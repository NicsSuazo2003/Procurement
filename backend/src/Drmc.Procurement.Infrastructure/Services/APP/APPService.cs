
using Drmc.Procurement.Application.Features.APP.DTO;
using Drmc.Procurement.Application.Features.APP.Interfaces;
using Drmc.Procurement.Infrastructure.Persistence;
using Drmc.Procurement.Domain.Entities.APP;
using APPEntity = Drmc.Procurement.Domain.Entities.APP.APP;

using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.APP;

public class APPService : IAPPService
{
    private readonly ApplicationDbContext _context;

    public APPService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<APPListDto>> GetAllAsync()
    {
        var apps = await _context.APPs
            .Include(x => x.Categories)
                .ThenInclude(x => x.Items)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return apps.Select(app => new APPListDto
        {
            APPId = app.APPId,
            PlanningYearId = app.PlanningYearId,
            APPReference = app.APPReference,
            Status = app.Status,
            RevisionNo = app.RevisionNo,
            TotalEstimatedBudget = app.Categories.Sum(
                category => category.TotalEstimatedBudget),
            CreatedAt = app.CreatedAt,
            CategoryCount = app.Categories.Count,
            ItemCount = app.Categories.Sum(
                category => category.Items.Count)
        }).ToList();
    }

    public async Task<APPDetailDto?> GetByIdAsync(int id)
    {
        var app = await _context.APPs
            .AsNoTracking()
            .Include(x => x.Categories)
                .ThenInclude(x => x.Items)
                    .ThenInclude(x => x.Sources)
                        .ThenInclude(x => x.PPMPItem)
                            .ThenInclude(x => x.PPMP)
            .Include(x => x.PreparedByUser)
            .Include(x => x.RecommendedByUser)
            .Include(x => x.ApprovedByUser)
            .FirstOrDefaultAsync(x => x.APPId == id);

        if (app == null)
        {
            return null;
        }

        return MapToDetailDto(app);
    }

    public async Task<GenerateAPPResultDto> GenerateAsync(
        int planningYearId,
        int createdBy)
    {
        var planningYear = await _context.PlanningYears
            .FirstOrDefaultAsync(x =>
                x.PlanningYearId == planningYearId);

        if (planningYear == null)
        {
            throw new InvalidOperationException(
                "The selected planning year does not exist.");
        }

        var creator = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == createdBy);

        if (creator == null || !creator.IsActive)
        {
            throw new InvalidOperationException(
                "The authenticated user does not exist or is inactive.");
        }

        // Initial business rule: one APP per planning year.
        var existingAPP = await _context.APPs
            .AnyAsync(x => x.PlanningYearId == planningYearId);

        if (existingAPP)
        {
            throw new InvalidOperationException(
                "An APP already exists for this planning year. " +
                "Use the existing APP to add subsequently reviewed PPMPs.");
        }

        // Only reviewed PPMPs for the selected planning year are eligible.
        var reviewedPPMPs = await _context.PPMPs
            .Include(x => x.Items)
            .Include(x => x.Department)
            .Where(x =>
                x.PlanningYearId == planningYearId &&
                x.Status == "Reviewed")
            .ToListAsync();

        if (reviewedPPMPs.Count == 0)
        {
            throw new InvalidOperationException(
                "There are no reviewed PPMPs available for APP generation.");
        }

        if (reviewedPPMPs.Any(x => x.Items.Count == 0))
        {
            throw new InvalidOperationException(
                "At least one reviewed PPMP has no items. " +
                "Please review the PPMP records before generating the APP.");
        }

        var now = DateTime.UtcNow;

        var app = new APPEntity
        {
            PlanningYearId = planningYearId,
            CreatedBy = createdBy,

            // Use the selected planning year, not the server's current year.
            APPReference =
                $"APP-{planningYear.Year}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",

            Status = APPStatuses.Draft,
            CreatedAt = now,
            Categories = []
        };

        // Consolidate into APP categories.
        // Keep each source PPMP item separate for traceability.
        var allPPMPItems = reviewedPPMPs
            .SelectMany(ppmp => ppmp.Items)
            .ToList();

        var groupedItems = allPPMPItems
            .GroupBy(item => item.CategoryId);

        foreach (var categoryGroup in groupedItems)
        {
            var appCategory = new APPCategory
            {
                CategoryId = categoryGroup.Key,
                TotalEstimatedBudget = 0,
                Items = []
            };

            foreach (var ppmpItem in categoryGroup)
            {
                var appItem = CreateAPPItemFromPPMPItem(
                    ppmpItem,
                    ppmpItem.PPMP.Department.DepartmentName);

                appCategory.Items.Add(appItem);

                appCategory.TotalEstimatedBudget +=
                    appItem.EstimatedBudget;
            }

            app.Categories.Add(appCategory);
        }

        _context.APPs.Add(app);

        // Mark source PPMPs as included in the APP.
        foreach (var ppmp in reviewedPPMPs)
        {
            ppmp.Status = "Included in APP";
            ppmp.UpdatedAt = now;
        }

        // SaveChanges is atomic and works with the configured SQL retry strategy.
        await _context.SaveChangesAsync();

        return BuildAPPResult(app, reviewedPPMPs.Count);
    }

    public async Task<GenerateAPPResultDto> AddReviewedPPMPsAsync(
        int appId,
        int addedBy)
    {
        // Load the existing APP and its categories/items.
        var app = await _context.APPs
            .Include(x => x.Categories)
                .ThenInclude(x => x.Items)
            .FirstOrDefaultAsync(x => x.APPId == appId);

        if (app == null)
        {
            throw new InvalidOperationException(
                "The APP record does not exist.");
        }

        // Do not modify an APP that has moved beyond Draft.
        if (app.Status != APPStatuses.Draft)
        {
            throw new InvalidOperationException(
                "Only Draft APP records can receive additional PPMPs.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == addedBy);

        if (user == null || !user.IsActive)
        {
            throw new InvalidOperationException(
                "The authenticated user does not exist or is inactive.");
        }

        // Find subsequently reviewed PPMPs for this APP's planning year.
        var reviewedPPMPs = await _context.PPMPs
            .Include(x => x.Items)
            .Include(x => x.Department)
            .Where(x =>
                x.PlanningYearId == app.PlanningYearId &&
                x.Status == "Reviewed")
            .ToListAsync();

        if (reviewedPPMPs.Count == 0)
        {
            throw new InvalidOperationException(
                "There are no additional reviewed PPMPs available for this APP.");
        }

        if (reviewedPPMPs.Any(x => x.Items.Count == 0))
        {
            throw new InvalidOperationException(
                "At least one reviewed PPMP has no items. " +
                "Please review the PPMP records before adding them.");
        }

        var ppmpItemIds = reviewedPPMPs
            .SelectMany(x => x.Items)
            .Select(x => x.PPMPItemId)
            .ToList();

        // Prevent a PPMP item from being linked to an APP more than once.
        var alreadyLinkedItemIds = await _context.APPItemSources
            .Where(x => ppmpItemIds.Contains(x.PPMPItemId))
            .Select(x => x.PPMPItemId)
            .ToListAsync();

        if (alreadyLinkedItemIds.Count > 0)
        {
            throw new InvalidOperationException(
                "One or more reviewed PPMP items are already linked to an APP. " +
                "Please check their PPMP statuses and APP source records.");
        }

        var now = DateTime.UtcNow;

        // Add each PPMP item to its matching APP category.
        foreach (var ppmp in reviewedPPMPs)
        {
            foreach (var ppmpItem in ppmp.Items)
            {
                var appCategory = app.Categories
                    .FirstOrDefault(x =>
                        x.CategoryId == ppmpItem.CategoryId);

                // Create the category only if this APP does not have it yet.
                if (appCategory == null)
                {
                    appCategory = new APPCategory
                    {
                        CategoryId = ppmpItem.CategoryId,
                        TotalEstimatedBudget = 0,
                        Items = []
                    };

                    app.Categories.Add(appCategory);
                }

                // Keep the source PPMP item as a separate APP item.
                var appItem = CreateAPPItemFromPPMPItem(
                    ppmpItem,
                    ppmp.Department.DepartmentName);

                appCategory.Items.Add(appItem);

                appCategory.TotalEstimatedBudget +=
                    appItem.EstimatedBudget;
            }

            ppmp.Status = "Included in APP";
            ppmp.UpdatedAt = now;
        }

        // SaveChanges is atomic and works with the configured SQL retry strategy.
        await _context.SaveChangesAsync();

        return BuildAPPResult(app, reviewedPPMPs.Count);
    }

    public Task<APPDetailDto> MarkPreparedAsync(int appId, int preparedBy) =>
        RecordApprovalAsync(
            appId,
            preparedBy,
            APPApprovalRoles.PreparedBy,
            APPStatuses.Draft,
            APPStatuses.Prepared,
            (query, at) => query.ExecuteUpdateAsync(setters => setters
                .SetProperty(app => app.PreparedByUserId, preparedBy)
                .SetProperty(app => app.PreparedAt, at)
                .SetProperty(app => app.Status, APPStatuses.Prepared)));

    public Task<APPDetailDto> MarkRecommendedAsync(int appId, int recommendedBy) =>
        RecordApprovalAsync(
            appId,
            recommendedBy,
            APPApprovalRoles.RecommendedBy,
            APPStatuses.Prepared,
            APPStatuses.Recommended,
            (query, at) => query.ExecuteUpdateAsync(setters => setters
                .SetProperty(app => app.RecommendedByUserId, recommendedBy)
                .SetProperty(app => app.RecommendedAt, at)
                .SetProperty(app => app.Status, APPStatuses.Recommended)));

    public Task<APPDetailDto> MarkApprovedAsync(int appId, int approvedBy) =>
        RecordApprovalAsync(
            appId,
            approvedBy,
            APPApprovalRoles.ApprovedBy,
            APPStatuses.Recommended,
            APPStatuses.Approved,
            (query, at) => query.ExecuteUpdateAsync(setters => setters
                .SetProperty(app => app.ApprovedByUserId, approvedBy)
                .SetProperty(app => app.ApprovedAt, at)
                .SetProperty(app => app.Status, APPStatuses.Approved)));

    private async Task<APPDetailDto> RecordApprovalAsync(
        int appId,
        int userId,
        string requiredRole,
        string expectedStatus,
        string nextStatus,
        Func<IQueryable<APPEntity>, DateTime, Task<int>> updateApproval)
    {
        var app = await _context.APPs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.APPId == appId);

        if (app is null)
        {
            throw new KeyNotFoundException("The APP record does not exist.");
        }

        if (app.Status != expectedStatus)
        {
            throw new InvalidOperationException(
                $"The APP must be in {expectedStatus} status before it can be marked {nextStatus}.");
        }

        var user = await _context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (user is null || !user.IsActive)
        {
            throw new InvalidOperationException(
                "The authenticated user does not exist or is inactive.");
        }

        if (!user.UserRoles.Any(x => x.Role.RoleName == requiredRole))
        {
            throw new InvalidOperationException(
                $"The user must have the {requiredRole} role to perform this APP approval.");
        }

        if (app.PreparedByUserId == userId ||
            app.RecommendedByUserId == userId ||
            app.ApprovedByUserId == userId)
        {
            throw new InvalidOperationException(
                "Each APP approval stage must be completed by a different user.");
        }

        var updateQuery = _context.APPs
            .Where(x =>
                x.APPId == appId &&
                x.Status == expectedStatus &&
                x.PreparedByUserId == app.PreparedByUserId &&
                x.RecommendedByUserId == app.RecommendedByUserId &&
                x.ApprovedByUserId == app.ApprovedByUserId);
        var updatedCount = await updateApproval(updateQuery, DateTime.UtcNow);

        if (updatedCount == 0)
        {
            throw new InvalidOperationException(
                "The APP changed while this sign-off was being recorded. Reload it and try again.");
        }

        return await GetByIdAsync(appId)
            ?? throw new InvalidOperationException(
                "The APP approval was saved, but its details could not be retrieved.");
    }

    private static APPItem CreateAPPItemFromPPMPItem(
        Drmc.Procurement.Domain.Entities.Procurement.PPMPItem ppmpItem,
        string endUser)
    {
        return new APPItem
        {
            ItemId = ppmpItem.ItemId,
            ItemCode = ppmpItem.ItemCode,
            ItemDescription = ppmpItem.ItemDescription,
            ItemType = ppmpItem.ItemType,
            Unit = ppmpItem.Unit,
            Specifications = ppmpItem.Specifications,
            PAPCode = ppmpItem.ItemCode,
            ProcurementProjectName = ppmpItem.ItemDescription,
            GeneralDescription = ppmpItem.GeneralDescriptionAndObjective,
            EndUser = endUser,
            Classification = ppmpItem.ProjectType,
            ModeOfProcurement = ppmpItem.RecommendedModeOfProcurement,
            StartOfProcurementActivity = ppmpItem.StartOfProcurementActivity,
            EndOfProcurementActivity = ppmpItem.EndOfProcurementActivity,
            SourceOfFunds = ppmpItem.SourceOfFunds,
            Remarks = ppmpItem.AttachedSupportingDocuments,
            TotalQuantity = ppmpItem.Quantity,
            EstimatedUnitPrice = ppmpItem.UnitPrice,
            EstimatedBudget = ppmpItem.EstimatedBudget,

            Sources =
            [
                new APPItemSource
                {
                    PPMPItemId = ppmpItem.PPMPItemId,
                    ContributedQuantity = ppmpItem.Quantity,
                    ContributedBudget = ppmpItem.EstimatedBudget
                }
            ]
        };
    }

    private static GenerateAPPResultDto BuildAPPResult(
        APPEntity app,
        int includedPPMPCount)
    {
        return new GenerateAPPResultDto
        {
            APPId = app.APPId,
            APPReference = app.APPReference,
            Status = app.Status,
            IncludedPPMPCount = includedPPMPCount,
            CategoryCount = app.Categories.Count,
            ItemCount = app.Categories.Sum(
                category => category.Items.Count),
            TotalEstimatedBudget = app.Categories.Sum(
                category => category.TotalEstimatedBudget)
        };
    }

    private static APPDetailDto MapToDetailDto(APPEntity app)
    {
        return new APPDetailDto
        {
            APPId = app.APPId,
            PlanningYearId = app.PlanningYearId,
            CreatedBy = app.CreatedBy,
            APPReference = app.APPReference,
            Status = app.Status,
            RevisionNo = app.RevisionNo,
            TotalEstimatedBudget = app.Categories.Sum(
                category => category.TotalEstimatedBudget),
            CreatedAt = app.CreatedAt,
            ApprovedAt = app.ApprovedAt,
            PreparedByUserId = app.PreparedByUserId,
            PreparedByName = app.PreparedByUser?.FullName,
            PreparedByPosition = app.PreparedByUser?.Position,
            PreparedAt = app.PreparedAt,
            RecommendedByUserId = app.RecommendedByUserId,
            RecommendedByName = app.RecommendedByUser?.FullName,
            RecommendedByPosition = app.RecommendedByUser?.Position,
            RecommendedAt = app.RecommendedAt,
            ApprovedByUserId = app.ApprovedByUserId,
            ApprovedByName = app.ApprovedByUser?.FullName,
            ApprovedByPosition = app.ApprovedByUser?.Position,

            Categories = app.Categories.Select(category =>
                new APPCategoryDto
                {
                    APPCategoryId = category.APPCategoryId,
                    CategoryId = category.CategoryId,
                    TotalEstimatedBudget =
                        category.TotalEstimatedBudget,

                    Items = category.Items.Select(item =>
                        new APPItemDto
                        {
                            APPItemId = item.APPItemId,
                            ItemId = item.ItemId,
                            ItemCode = item.ItemCode,
                            ItemDescription = item.ItemDescription,
                            ItemType = item.ItemType,
                            Unit = item.Unit,
                            Specifications = item.Specifications,
                            PAPCode = item.PAPCode,
                            ObjectCode = item.ObjectCode,
                            ProcurementProjectName = item.ProcurementProjectName,
                            GeneralDescription = item.GeneralDescription,
                            EndUser = item.EndUser,
                            Classification = item.Classification,
                            ModeOfProcurement = item.ModeOfProcurement,
                            BidEvaluationCriteria = item.BidEvaluationCriteria,
                            MarginalizedSectorConsideration = item.MarginalizedSectorConsideration,
                            GreenTechnicalSpecifications = item.GreenTechnicalSpecifications,
                            TargetPRSubmissionDate = item.TargetPRSubmissionDate,
                            StartOfProcurementActivity = item.StartOfProcurementActivity,
                            EndOfProcurementActivity = item.EndOfProcurementActivity,
                            SourceOfFunds = item.SourceOfFunds,
                            MOOE = item.MOOE,
                            CapitalOutlay = item.CapitalOutlay,
                            MultiYearContractCost = item.MultiYearContractCost,
                            LifeCycleCost = item.LifeCycleCost,
                            ProcurementStrategyTools = item.ProcurementStrategyTools,
                            ProspectiveBidders = item.ProspectiveBidders,
                            PotentialObservers = item.PotentialObservers,
                            ProcurementAgent = item.ProcurementAgent,
                            CollaboratingEntities = item.CollaboratingEntities,
                            Remarks = item.Remarks,
                            TotalQuantity = item.TotalQuantity,
                            EstimatedUnitPrice =
                                item.EstimatedUnitPrice,
                            EstimatedBudget = item.EstimatedBudget,

                            Sources = item.Sources.Select(source =>
                                new APPItemSourceDto
                                {
                                    APPItemSourceId =
                                        source.APPItemSourceId,
                                    PPMPItemId = source.PPMPItemId,
                                    PPMPId =
                                        source.PPMPItem.PPMPId,
                                    PPMPReference =
                                        source.PPMPItem.PPMP.PPMPReference,
                                    ContributedQuantity =
                                        source.ContributedQuantity,
                                    ContributedBudget =
                                        source.ContributedBudget
                                }).ToList()
                        }).ToList()
                }).ToList()
        };
    }
}
