
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Audit;
using Drmc.Procurement.Domain.Entities.Procurement;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.PPMPs;

public class PPMPService : IPPMPService
{
    private readonly ApplicationDbContext _context;

    public PPMPService(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // SHARED LIST PROJECTION
    // =========================================================

    private static IQueryable<PPMPDto> ProjectList(IQueryable<PPMP> query)
    {
        return query.Select(x => new PPMPDto
        {
            PPMPId = x.PPMPId,
            PPMPReference = x.PPMPReference,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department.DepartmentName,
            PlanningYearId = x.PlanningYearId,
            PlanningYear = x.PlanningYear.Year,
            CreatedBy = x.CreatedBy,
            CreatedByName = x.Creator.FullName,
            Status = x.Status,
            EstimatedBudget = x.EstimatedBudget,
            CreatedAt = x.CreatedAt,
            SubmittedAt = x.SubmittedAt,
            UpdatedAt = x.UpdatedAt,
            ItemCount = x.Items.Count()
        });
    }

    // =========================================================
    // GET ALL PPMPs
    // =========================================================

    public async Task<List<PPMPDto>> GetAllAsync(int? departmentId)
    {
        var query = _context.PPMPs
            .AsNoTracking()
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(x =>
                x.DepartmentId == departmentId.Value);
        }

        return await ProjectList(query).ToListAsync();
    }

    // =========================================================
    // GET PAGED PPMPs
    // =========================================================

    public async Task<PagedResult<PPMPDto>> GetPagedAsync(
        int? departmentId,
        int page,
        int pageSize,
        string[]? statuses,
        int? planningYear)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 25;
        if (pageSize > 100) pageSize = 100;

        var query = _context.PPMPs
            .AsNoTracking()
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(x =>
                x.DepartmentId == departmentId.Value);
        }

        if (planningYear.HasValue)
        {
            query = query.Where(x =>
                x.PlanningYear.Year == planningYear.Value);
        }

        var normalizedStatuses = (statuses ?? Array.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedStatuses.Length > 0)
        {
            query = query.Where(x =>
                normalizedStatuses.Contains(x.Status));
        }

        var totalCount = await query.CountAsync();

        var items = await ProjectList(query)
            .OrderByDescending(x => x.SubmittedAt)
            .ThenByDescending(x => x.PPMPId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<PPMPDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // =========================================================
    // GET SUMMARY
    // =========================================================

    public async Task<PPPMSummaryDto> GetSummaryAsync(int? departmentId)
    {
        var query = _context.PPMPs
            .AsNoTracking()
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(x =>
                x.DepartmentId == departmentId.Value);
        }

        var countsByStatus = await query
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var byStatus = countsByStatus
            .ToDictionary(
                x => x.Status,
                x => x.Count,
                StringComparer.OrdinalIgnoreCase);

        static int Get(Dictionary<string, int> dict, string key) =>
            dict.TryGetValue(key, out var v) ? v : 0;

        var awaitingAction =
            Get(byStatus, "For Unit Head Approval") +
            Get(byStatus, "Approved by Unit Head") +
            Get(byStatus, "Under Review");

        var completedReview =
            Get(byStatus, "Reviewed") +
            Get(byStatus, "Included in APP");

        return new PPPMSummaryDto
        {
            Total = byStatus.Values.Sum(),
            AwaitingAction = awaitingAction,
            CompletedReview = completedReview,
            ByStatus = byStatus
        };
    }

    // =========================================================
    // GET PPMP BY ID
    // =========================================================

    public async Task<PPMPDetailDto?> GetByIdAsync(
        int id,
        int? departmentId)
    {
        return await _context.PPMPs
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x =>
                x.PPMPId == id &&
                (!departmentId.HasValue ||
                 x.DepartmentId == departmentId.Value))
            .Select(x => new PPMPDetailDto
            {
                PPMPId = x.PPMPId,
                PPMPReference = x.PPMPReference,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department.DepartmentName,
                PlanningYearId = x.PlanningYearId,
                PlanningYear = x.PlanningYear.Year,
                CreatedBy = x.CreatedBy,
                CreatedByName = x.Creator.FullName,
                Status = x.Status,
                EstimatedBudget = x.EstimatedBudget,
                CreatedAt = x.CreatedAt,
                SubmittedAt = x.SubmittedAt,
                UpdatedAt = x.UpdatedAt,

                Items = x.Items
                    .Select(item => new PPMPItemDto
                    {
                        PPMPItemId = item.PPMPItemId,
                        ItemId = item.ItemId,
                        CategoryId = item.CategoryId,
                        CategoryName = item.Category.CategoryName,

                        ItemCode = item.ItemCode,
                        ItemDescription = item.ItemDescription,
                        ItemType = item.ItemType,
                        Unit = item.Unit,
                        Specifications = item.Specifications,

                        GeneralDescriptionAndObjective =
                            item.GeneralDescriptionAndObjective,

                        ProjectType = item.ProjectType,

                        PreProcurementConference =
                            item.PreProcurementConference,

                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        EstimatedBudget = item.EstimatedBudget,

                        SectionAllocations = item.SectionAllocations
                            .Select(allocation => new PPMPSectionAllocationDto
                            {
                                AllocationId = allocation.AllocationId,
                                SectionId = allocation.SectionId,
                                SectionCode = allocation.Section.SectionCode,
                                SectionName = allocation.Section.SectionName,
                                Quantity = allocation.Quantity
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    // =========================================================
    // GET PPMP CHANGE HISTORY
    // =========================================================

    public async Task<List<PPMPChangeHistoryDto>> GetChangeHistoryAsync(
        int id,
        int? departmentId)
    {
        return await _context.PPMPChangeHistories
            .AsNoTracking()
            .AsSplitQuery()
            .Where(history =>
                history.PPMPId == id &&
                (!departmentId.HasValue ||
                 history.PPMP.DepartmentId == departmentId.Value))
            .OrderByDescending(history => history.ChangedAt)
            .ThenByDescending(history => history.ChangeHistoryId)
            .Select(history => new PPMPChangeHistoryDto
            {
                ChangeHistoryId = history.ChangeHistoryId,
                PPMPId = history.PPMPId,

                ChangedBy = history.ChangedBy,
                ChangedByName = history.ChangedByUser.FullName,

                Reason = history.Reason,

                PreviousBudget = history.PreviousBudget,
                UpdatedBudget = history.UpdatedBudget,

                ChangedAt = history.ChangedAt,

                Items = history.Items
                    .Select(item => new PPMPChangeHistoryItemDto
                    {
                        ChangeHistoryItemId = item.ChangeHistoryItemId,

                        PreviousItemId = item.PreviousItemId,
                        PreviousItemCode = item.PreviousItemCode,
                        PreviousDescription = item.PreviousDescription,
                        PreviousUnit = item.PreviousUnit,
                        PreviousQuantity = item.PreviousQuantity,
                        PreviousUnitPrice = item.PreviousUnitPrice,
                        PreviousEstimatedBudget = item.PreviousEstimatedBudget,

                        PreviousGeneralDescriptionAndObjective =
                            item.PreviousGeneralDescriptionAndObjective,

                        PreviousProjectType =
                            item.PreviousProjectType,

                        PreviousPreProcurementConference =
                            item.PreviousPreProcurementConference,

                        UpdatedItemId = item.UpdatedItemId,
                        UpdatedItemCode = item.UpdatedItemCode,
                        UpdatedDescription = item.UpdatedDescription,
                        UpdatedUnit = item.UpdatedUnit,
                        UpdatedQuantity = item.UpdatedQuantity,
                        UpdatedUnitPrice = item.UpdatedUnitPrice,
                        UpdatedEstimatedBudget = item.UpdatedEstimatedBudget,

                        UpdatedGeneralDescriptionAndObjective =
                            item.UpdatedGeneralDescriptionAndObjective,

                        UpdatedProjectType =
                            item.UpdatedProjectType,

                        UpdatedPreProcurementConference =
                            item.UpdatedPreProcurementConference,

                        SectionAllocations = item.SectionAllocations
                            .Select(allocation =>
                                new PPMPChangeHistorySectionAllocationDto
                                {
                                    ChangeHistorySectionAllocationId =
                                        allocation.ChangeHistorySectionAllocationId,

                                    PreviousSectionId =
                                        allocation.PreviousSectionId,

                                    PreviousSectionCode =
                                        allocation.PreviousSectionCode,

                                    PreviousSectionName =
                                        allocation.PreviousSectionName,

                                    PreviousQuantity =
                                        allocation.PreviousQuantity,

                                    UpdatedSectionId =
                                        allocation.UpdatedSectionId,

                                    UpdatedSectionCode =
                                        allocation.UpdatedSectionCode,

                                    UpdatedSectionName =
                                        allocation.UpdatedSectionName,

                                    UpdatedQuantity =
                                        allocation.UpdatedQuantity
                                })
                            .ToList()
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    // =========================================================
    // CREATE PPMP
    // =========================================================

    public async Task<PPMPDetailDto> CreateAsync(
        CreatePPMPRequest request,
        int createdBy,
        int departmentId)
    {
        if (request.DepartmentId != departmentId)
        {
            throw new InvalidOperationException(
                "You can only create a PPMP for your own department.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "A PPMP must contain at least one item.");
        }

        var department = await _context.Departments
            .FirstOrDefaultAsync(x =>
                x.DepartmentId == request.DepartmentId &&
                x.IsActive);

        if (department == null)
        {
            throw new InvalidOperationException(
                "The selected department does not exist or is inactive.");
        }

        var planningYear = await _context.PlanningYears
            .FirstOrDefaultAsync(x =>
                x.PlanningYearId == request.PlanningYearId);

        if (planningYear == null)
        {
            throw new InvalidOperationException(
                "The selected planning year does not exist.");
        }

        if (!string.Equals(
                planningYear.Status,
                "Open",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The selected planning year is not open.");
        }

        var creator = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.UserId == createdBy &&
                x.IsActive);

        if (creator == null)
        {
            throw new InvalidOperationException(
                "The selected creator does not exist or is inactive.");
        }

        if (creator.DepartmentId != departmentId)
        {
            throw new InvalidOperationException(
                "The creator does not belong to the selected department.");
        }

        var submittedCategoryIds = request.Items
            .Select(i => i.CategoryId)
            .Distinct()
            .ToHashSet();

        var validCategoryIds = await _context.PlanningYearCategories
            .Where(x =>
                x.PlanningYearId == request.PlanningYearId &&
                x.IsActive &&
                x.Category.IsActive &&
                submittedCategoryIds.Contains(x.CategoryId))
            .Select(x => x.CategoryId)
            .ToListAsync();

        var invalidCategories = submittedCategoryIds
            .Except(validCategoryIds)
            .ToList();

        if (invalidCategories.Count > 0)
        {
            throw new InvalidOperationException(
                $"Category ID {invalidCategories[0]} is inactive or not assigned to the selected planning year.");
        }

        var submittedItemIds = request.Items
            .Where(i => i.ItemId.HasValue)
            .Select(i => i.ItemId!.Value)
            .Distinct()
            .ToHashSet();

        var catalogueItems = submittedItemIds.Count == 0
            ? new Dictionary<int, Item>()
            : (await _context.Items
                .Where(x =>
                    x.IsActive &&
                    submittedItemIds.Contains(x.ItemId))
                .ToDictionaryAsync(x => x.ItemId));

        var missingItemIds = submittedItemIds
            .Except(catalogueItems.Keys)
            .ToList();

        if (missingItemIds.Count > 0)
        {
            throw new InvalidOperationException(
                $"Catalogue item ID {missingItemIds[0]} does not exist or is inactive.");
        }

        var submittedSectionIds = request.Items
            .SelectMany(i => i.SectionAllocations ?? new List<CreatePPMPSectionAllocationRequest>())
            .Select(a => a.SectionId)
            .Distinct()
            .ToHashSet();

        var validSectionIds = await _context.DepartmentSections
            .Where(x =>
                x.DepartmentId == request.DepartmentId &&
                x.IsActive &&
                submittedSectionIds.Contains(x.SectionId))
            .Select(x => x.SectionId)
            .ToListAsync();

        var invalidSections = submittedSectionIds
            .Except(validSectionIds)
            .ToList();

        if (invalidSections.Count > 0)
        {
            throw new InvalidOperationException(
                $"Section ID {invalidSections[0]} does not exist, is inactive, or does not belong to the selected department.");
        }

        var ppmp = new PPMP
        {
            PPMPReference =
                $"PPMP-{planningYear.Year}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",

            DepartmentId = request.DepartmentId,
            PlanningYearId = request.PlanningYearId,
            CreatedBy = createdBy,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            EstimatedBudget = 0
        };

        decimal totalBudget = 0;

        foreach (var itemRequest in request.Items)
        {
            if (decimal.Round(itemRequest.Quantity, 2) != itemRequest.Quantity ||
                decimal.Round(itemRequest.UnitPrice, 2) != itemRequest.UnitPrice)
            {
                throw new InvalidOperationException(
                    "Quantity and unit price must have no more than two decimal places.");
            }

            string itemCode;
            string itemDescription;
            string? itemType;
            string unit;
            string? specifications;

            int? itemId = itemRequest.ItemId;

            if (itemRequest.ItemId.HasValue)
            {
                var catalogueItem = catalogueItems[itemRequest.ItemId.Value];

                if (catalogueItem.CategoryId != itemRequest.CategoryId)
                {
                    throw new InvalidOperationException(
                        "The selected catalogue item does not belong to the selected category.");
                }

                itemCode = catalogueItem.ItemCode;
                itemDescription = catalogueItem.ItemName;
                itemType = catalogueItem.ItemType;
                unit = catalogueItem.Unit;
                specifications = catalogueItem.Specifications;
            }
            else
            {
                itemCode = itemRequest.ItemCode?.Trim() ?? string.Empty;
                itemDescription = itemRequest.ItemDescription.Trim();
                itemType = itemRequest.ItemType?.Trim();
                unit = itemRequest.Unit.Trim();
                specifications = itemRequest.Specifications?.Trim();
            }

            var generalDescriptionAndObjective =
                itemRequest.GeneralDescriptionAndObjective.Trim();

            var projectType = itemRequest.ProjectType.Trim();

            if (string.IsNullOrWhiteSpace(generalDescriptionAndObjective))
            {
                throw new InvalidOperationException(
                    "General description and objective are required.");
            }

            if (string.IsNullOrWhiteSpace(projectType))
            {
                throw new InvalidOperationException(
                    "Project type is required.");
            }

            if (string.IsNullOrWhiteSpace(itemDescription) ||
                string.IsNullOrWhiteSpace(unit))
            {
                throw new InvalidOperationException(
                    "Item description and unit are required.");
            }

            if (itemRequest.SectionAllocations == null ||
                itemRequest.SectionAllocations.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Item '{itemDescription}' must have at least one section allocation.");
            }

            var duplicateSection = itemRequest.SectionAllocations
                .GroupBy(x => x.SectionId)
                .Any(group => group.Count() > 1);

            if (duplicateSection)
            {
                throw new InvalidOperationException(
                    $"Item '{itemDescription}' contains duplicate section allocations.");
            }

            var allocationTotal = itemRequest.SectionAllocations
                .Sum(x => x.Quantity);

            if (allocationTotal != itemRequest.Quantity)
            {
                throw new InvalidOperationException(
                    $"Section allocation quantities for '{itemDescription}' must equal its total quantity.");
            }

            foreach (var allocation in itemRequest.SectionAllocations)
            {
                if (allocation.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        "Section allocation quantities must be greater than zero.");
                }

                if (decimal.Round(allocation.Quantity, 2) != allocation.Quantity)
                {
                    throw new InvalidOperationException(
                        "Section allocation quantities must have no more than two decimal places.");
                }
            }

            var itemBudget = itemRequest.Quantity * itemRequest.UnitPrice;

            var ppmpItem = new PPMPItem
            {
                ItemId = itemId,
                CategoryId = itemRequest.CategoryId,

                ItemCode = itemCode,
                ItemDescription = itemDescription,
                ItemType = itemType,
                Unit = unit,
                Specifications = specifications,

                GeneralDescriptionAndObjective =
                    generalDescriptionAndObjective,

                ProjectType = projectType,

                PreProcurementConference =
                    itemRequest.PreProcurementConference,

                Quantity = itemRequest.Quantity,
                UnitPrice = itemRequest.UnitPrice,
                EstimatedBudget = itemBudget
            };

            foreach (var allocation in itemRequest.SectionAllocations)
            {
                ppmpItem.SectionAllocations.Add(
                    new PPMPSectionAllocation
                    {
                        SectionId = allocation.SectionId,
                        Quantity = allocation.Quantity
                    });
            }

            ppmp.Items.Add(ppmpItem);
            totalBudget += itemBudget;
        }

        ppmp.EstimatedBudget = totalBudget;

        // A single SaveChangesAsync is automatically transactional.
        _context.PPMPs.Add(ppmp);

        await _context.SaveChangesAsync();

        var createdPPMP = await GetByIdAsync(
            ppmp.PPMPId,
            departmentId);

        if (createdPPMP == null)
        {
            throw new InvalidOperationException(
                "The PPMP was saved, but its details could not be retrieved.");
        }

        return createdPPMP;
    }

    // =========================================================
    // SUBMIT PPMP
    // =========================================================

    public async Task<PPMPDetailDto> SubmitAsync(
        int id,
        int userId,
        int departmentId)
    {
        var ppmp = await _context.PPMPs
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x =>
                x.PPMPId == id &&
                x.DepartmentId == departmentId &&
                x.CreatedBy == userId);

        if (ppmp == null)
        {
            throw new InvalidOperationException(
                $"PPMP with ID {id} was not found.");
        }

        if (ppmp.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft PPMPs can be submitted.");
        }

        if (ppmp.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "A PPMP must contain at least one item before submission.");
        }

        ppmp.Status = "For Unit Head Approval";
        ppmp.SubmittedAt = DateTime.UtcNow;
        ppmp.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var submittedPPMP = await GetByIdAsync(id, departmentId);
        if (submittedPPMP == null)
        {
            throw new InvalidOperationException(
                "PPMP was submitted, but its details could not be retrieved.");
        }

        return submittedPPMP;
    }

    // =========================================================
    // APPROVE PPMP
    // =========================================================

    public async Task<PPMPDetailDto> ApproveAsync(
        int id,
        ApprovePPMPRequest request,
        int approvedBy,
        int departmentId)
    {
        var ppmp = await _context.PPMPs
            .FirstOrDefaultAsync(x =>
                x.PPMPId == id &&
                x.DepartmentId == departmentId);

        if (ppmp == null)
        {
            throw new InvalidOperationException(
                $"PPMP with ID {id} was not found.");
        }

        if (ppmp.Status != "For Unit Head Approval")
        {
            throw new InvalidOperationException(
                "Only PPMPs awaiting Unit Head approval can be approved.");
        }

        var approver = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.UserId == approvedBy &&
                x.IsActive);

        if (approver == null)
        {
            throw new InvalidOperationException(
                "The selected approver does not exist or is inactive.");
        }

        if (approver.DepartmentId != ppmp.DepartmentId)
        {
            throw new InvalidOperationException(
                "The approver does not belong to the PPMP department.");
        }

        var approval = new PPMPApproval
        {
            PPMPId = ppmp.PPMPId,
            ApprovedBy = approvedBy,
            ApprovalType = "Unit Head Approval",
            ApprovedAt = DateTime.UtcNow,
            Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                ? null
                : request.Remarks.Trim()
        };

        _context.PPMPApprovals.Add(approval);

        ppmp.Status = "Approved by Unit Head";
        ppmp.UpdatedAt = DateTime.UtcNow;

        // Approval record and status update are saved atomically.
        await _context.SaveChangesAsync();

        var approvedPPMP = await GetByIdAsync(id, departmentId);
        if (approvedPPMP == null)
        {
            throw new InvalidOperationException(
                "PPMP was approved, but its details could not be retrieved.");
        }

        return approvedPPMP;
    }

    // =========================================================
    // START PROCUREMENT REVIEW
    // =========================================================

    public async Task<PPMPDetailDto> StartReviewAsync(int id)
    {
        var ppmp = await _context.PPMPs
            .FirstOrDefaultAsync(x => x.PPMPId == id);

        if (ppmp == null)
        {
            throw new InvalidOperationException(
                $"PPMP with ID {id} was not found.");
        }

        if (ppmp.Status != "Approved by Unit Head")
        {
            throw new InvalidOperationException(
                "Only PPMPs approved by the Unit Head can enter Procurement review.");
        }

        ppmp.Status = "Under Review";
        ppmp.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var reviewedPPMP = await GetByIdAsync(id, null);
        if (reviewedPPMP == null)
        {
            throw new InvalidOperationException(
                "PPMP review was started, but its details could not be retrieved.");
        }

        return reviewedPPMP;
    }

    // =========================================================
    // COMPLETE PROCUREMENT REVIEW
    // =========================================================

    public async Task<PPMPDetailDto> CompleteReviewAsync(
        int id,
        int reviewedBy)
    {
        var ppmp = await _context.PPMPs
            .FirstOrDefaultAsync(x => x.PPMPId == id);

        if (ppmp == null)
        {
            throw new InvalidOperationException(
                $"PPMP with ID {id} was not found.");
        }

        if (ppmp.Status != "Under Review" &&
            ppmp.Status != "Updated")
        {
            throw new InvalidOperationException(
                "Only PPMPs under review or already updated can be marked as Reviewed.");
        }

        var reviewer = await _context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.UserId == reviewedBy &&
                x.IsActive);

        if (reviewer == null)
        {
            throw new InvalidOperationException(
                "The reviewer does not exist or is inactive.");
        }

        var isProcurementStaff = reviewer.UserRoles
            .Any(x => x.Role.RoleName == "Procurement Staff");

        if (!isProcurementStaff)
        {
            throw new InvalidOperationException(
                "Only Procurement Staff can complete a PPMP review.");
        }

        ppmp.Status = "Reviewed";
        ppmp.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(id, null);
        if (result == null)
        {
            throw new InvalidOperationException(
                "The PPMP was marked as Reviewed, but its details could not be retrieved.");
        }

        return result;
    }

    // =========================================================
    // UPDATE PPMP - PROCUREMENT
    // =========================================================

    public async Task<PPMPDetailDto> UpdateAsync(
        int id,
        UpdatePPMPRequest request,
        int changedBy)
    {
        var ppmp = await _context.PPMPs
            .Include(x => x.Items)
                .ThenInclude(x => x.SectionAllocations)
                    .ThenInclude(x => x.Section)
            .FirstOrDefaultAsync(x => x.PPMPId == id);

        if (ppmp == null)
        {
            throw new InvalidOperationException(
                $"PPMP with ID {id} was not found.");
        }

        if (ppmp.Status != "Under Review")
        {
            throw new InvalidOperationException(
                "Only PPMPs under Procurement review can be updated.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "A PPMP must contain at least one item.");
        }

        var changingUser = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.UserId == changedBy &&
                x.IsActive);

        if (changingUser == null)
        {
            throw new InvalidOperationException(
                "The selected user does not exist or is inactive.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException(
                "A reason for the changes is required.");
        }

        if (request.Reason.Trim().Length > 1000)
        {
            throw new InvalidOperationException(
                "The change reason cannot exceed 1000 characters.");
        }

        var duplicateItemIds = request.Items
            .Where(x => x.PPMPItemId.HasValue)
            .GroupBy(x => x.PPMPItemId)
            .Any(x => x.Count() > 1);

        if (duplicateItemIds)
        {
            throw new InvalidOperationException(
                "The update contains duplicate PPMP item IDs.");
        }

        var existingItemIds = ppmp.Items
            .Select(x => x.PPMPItemId)
            .ToHashSet();

        if (request.Items.Any(x =>
            x.PPMPItemId.HasValue &&
            !existingItemIds.Contains(x.PPMPItemId.Value)))
        {
            throw new InvalidOperationException(
                "One or more PPMP item IDs do not belong to this PPMP.");
        }

        var submittedCategoryIds = request.Items
            .Select(i => i.CategoryId)
            .Distinct()
            .ToHashSet();

        var validCategoryIds = await _context.PlanningYearCategories
            .Where(x =>
                x.PlanningYearId == ppmp.PlanningYearId &&
                x.IsActive &&
                x.Category.IsActive &&
                submittedCategoryIds.Contains(x.CategoryId))
            .Select(x => x.CategoryId)
            .ToListAsync();

        var invalidCategories = submittedCategoryIds
            .Except(validCategoryIds)
            .ToList();

        if (invalidCategories.Count > 0)
        {
            throw new InvalidOperationException(
                $"Category ID {invalidCategories[0]} is inactive or not assigned to this planning year.");
        }

        var submittedItemIds = request.Items
            .Where(i => i.ItemId.HasValue)
            .Select(i => i.ItemId!.Value)
            .Distinct()
            .ToHashSet();

        var catalogueItems = submittedItemIds.Count == 0
            ? new Dictionary<int, Item>()
            : (await _context.Items
                .Where(x =>
                    x.IsActive &&
                    submittedItemIds.Contains(x.ItemId))
                .ToDictionaryAsync(x => x.ItemId));

        var missingItemIds = submittedItemIds
            .Except(catalogueItems.Keys)
            .ToList();

        if (missingItemIds.Count > 0)
        {
            throw new InvalidOperationException(
                $"Catalogue item ID {missingItemIds[0]} does not exist or is inactive.");
        }

        var submittedSectionIds = request.Items
            .SelectMany(i => i.SectionAllocations ?? new List<UpdatePPMPSectionAllocationRequest>())
            .Select(a => a.SectionId)
            .Distinct()
            .ToHashSet();

        var validSectionIds = await _context.DepartmentSections
            .Where(x =>
                x.DepartmentId == ppmp.DepartmentId &&
                x.IsActive &&
                submittedSectionIds.Contains(x.SectionId))
            .Select(x => x.SectionId)
            .ToListAsync();

        var invalidSections = submittedSectionIds
            .Except(validSectionIds)
            .ToList();

        if (invalidSections.Count > 0)
        {
            throw new InvalidOperationException(
                $"Section ID {invalidSections[0]} is invalid or inactive.");
        }

        var sectionLookup = await _context.DepartmentSections
            .Where(x => validSectionIds.Contains(x.SectionId))
            .ToDictionaryAsync(
                x => x.SectionId,
                x => new { x.SectionCode, x.SectionName });

        var oldBudget = ppmp.EstimatedBudget;
        decimal newBudget = 0;

        var history = new PPMPChangeHistory
        {
            PPMPId = ppmp.PPMPId,
            ChangedBy = changedBy,
            Reason = request.Reason.Trim(),
            PreviousBudget = oldBudget,
            UpdatedBudget = 0,
            ChangedAt = DateTime.UtcNow
        };

        var changedItemCount = 0;

        var originalItems = ppmp.Items.ToList();

        foreach (var itemRequest in request.Items)
        {
            if (itemRequest.Quantity <= 0 || itemRequest.UnitPrice < 0)
            {
                throw new InvalidOperationException(
                    "Quantity must be greater than zero and unit price cannot be negative.");
            }

            if (decimal.Round(itemRequest.Quantity, 2) != itemRequest.Quantity ||
                decimal.Round(itemRequest.UnitPrice, 2) != itemRequest.UnitPrice)
            {
                throw new InvalidOperationException(
                    "Quantity and unit price must have no more than two decimal places.");
            }

            string itemCode;
            string description;
            string? itemType;
            string unit;
            string? specifications;

            if (itemRequest.ItemId.HasValue)
            {
                var catalogueItem = catalogueItems[itemRequest.ItemId.Value];

                if (catalogueItem.CategoryId != itemRequest.CategoryId)
                {
                    throw new InvalidOperationException(
                        "The catalogue item does not belong to the selected category.");
                }

                itemCode = catalogueItem.ItemCode;
                description = catalogueItem.ItemName;
                itemType = catalogueItem.ItemType;
                unit = catalogueItem.Unit;
                specifications = catalogueItem.Specifications;
            }
            else
            {
                itemCode = itemRequest.ItemCode?.Trim() ?? string.Empty;
                description = itemRequest.ItemDescription.Trim();
                itemType = itemRequest.ItemType?.Trim();
                unit = itemRequest.Unit.Trim();
                specifications = itemRequest.Specifications?.Trim();
            }

            var generalDescriptionAndObjective =
                itemRequest.GeneralDescriptionAndObjective.Trim();

            var projectType = itemRequest.ProjectType.Trim();

            if (string.IsNullOrWhiteSpace(generalDescriptionAndObjective))
            {
                throw new InvalidOperationException(
                    "General description and objective are required.");
            }

            if (string.IsNullOrWhiteSpace(projectType))
            {
                throw new InvalidOperationException(
                    "Project type is required.");
            }

            if (string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(unit))
            {
                throw new InvalidOperationException(
                    "Item description and unit are required.");
            }

            if (itemRequest.SectionAllocations == null ||
                itemRequest.SectionAllocations.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Item '{description}' must have at least one section allocation.");
            }

            if (itemRequest.SectionAllocations
                .GroupBy(x => x.SectionId)
                .Any(x => x.Count() > 1))
            {
                throw new InvalidOperationException(
                    $"Item '{description}' contains duplicate section allocations.");
            }

            if (itemRequest.SectionAllocations.Any(x => x.Quantity <= 0))
            {
                throw new InvalidOperationException(
                    "Section allocation quantities must be greater than zero.");
            }

            if (itemRequest.SectionAllocations.Any(x =>
                decimal.Round(x.Quantity, 2) != x.Quantity))
            {
                throw new InvalidOperationException(
                    "Section allocation quantities must have no more than two decimal places.");
            }

            if (itemRequest.SectionAllocations.Sum(x => x.Quantity)
                != itemRequest.Quantity)
            {
                throw new InvalidOperationException(
                    $"Section allocations for '{description}' must equal the total item quantity.");
            }

            var itemBudget = itemRequest.Quantity * itemRequest.UnitPrice;
            newBudget += itemBudget;

            var existingItem = itemRequest.PPMPItemId.HasValue
                ? ppmp.Items.First(x =>
                    x.PPMPItemId == itemRequest.PPMPItemId.Value)
                : null;

            var previousAllocations =
                new Dictionary<int, (decimal Quantity, string SectionCode, string SectionName)>();

            if (existingItem != null)
            {
                foreach (var allocation in existingItem.SectionAllocations)
                {
                    previousAllocations[allocation.SectionId] = (
                        allocation.Quantity,
                        allocation.Section.SectionCode,
                        allocation.Section.SectionName
                    );
                }
            }

            bool itemChanged;

            if (existingItem == null)
            {
                itemChanged = true;
            }
            else
            {
                itemChanged =
                    existingItem.ItemId != itemRequest.ItemId ||
                    existingItem.CategoryId != itemRequest.CategoryId ||
                    existingItem.ItemCode != itemCode ||
                    existingItem.ItemDescription != description ||
                    existingItem.ItemType != itemType ||
                    existingItem.Unit != unit ||
                    existingItem.Specifications != specifications ||
                    existingItem.GeneralDescriptionAndObjective != generalDescriptionAndObjective ||
                    existingItem.ProjectType != projectType ||
                    existingItem.PreProcurementConference != itemRequest.PreProcurementConference ||
                    existingItem.Quantity != itemRequest.Quantity ||
                    existingItem.UnitPrice != itemRequest.UnitPrice ||
                    existingItem.EstimatedBudget != itemBudget;
            }

            var allocationChanged = existingItem != null &&
                (
                    existingItem.SectionAllocations.Count !=
                        itemRequest.SectionAllocations.Count ||
                    itemRequest.SectionAllocations.Any(newAllocation =>
                        !previousAllocations.ContainsKey(newAllocation.SectionId) ||
                        previousAllocations[newAllocation.SectionId].Quantity
                            != newAllocation.Quantity)
                );

            if (!itemChanged && !allocationChanged)
            {
                continue;
            }

            changedItemCount++;

            var historyItem = new PPMPChangeHistoryItem();

            if (existingItem != null)
            {
                historyItem.PreviousItemId = existingItem.ItemId;
                historyItem.PreviousItemCode = existingItem.ItemCode;
                historyItem.PreviousDescription = existingItem.ItemDescription;
                historyItem.PreviousUnit = existingItem.Unit;
                historyItem.PreviousQuantity = existingItem.Quantity;
                historyItem.PreviousUnitPrice = existingItem.UnitPrice;
                historyItem.PreviousEstimatedBudget = existingItem.EstimatedBudget;

                historyItem.PreviousGeneralDescriptionAndObjective =
                    existingItem.GeneralDescriptionAndObjective;

                historyItem.PreviousProjectType =
                    existingItem.ProjectType;

                historyItem.PreviousPreProcurementConference =
                    existingItem.PreProcurementConference;
            }

            historyItem.UpdatedItemId = itemRequest.ItemId;
            historyItem.UpdatedItemCode = itemCode;
            historyItem.UpdatedDescription = description;
            historyItem.UpdatedUnit = unit;
            historyItem.UpdatedQuantity = itemRequest.Quantity;
            historyItem.UpdatedUnitPrice = itemRequest.UnitPrice;
            historyItem.UpdatedEstimatedBudget = itemBudget;

            historyItem.UpdatedGeneralDescriptionAndObjective =
                generalDescriptionAndObjective;

            historyItem.UpdatedProjectType = projectType;

            historyItem.UpdatedPreProcurementConference =
                itemRequest.PreProcurementConference;

            var previousSectionIds = previousAllocations.Keys.ToHashSet();

            var updatedSectionIds = itemRequest.SectionAllocations
                .Select(x => x.SectionId)
                .ToHashSet();

            foreach (var sectionId in previousSectionIds.Union(updatedSectionIds))
            {
                var hasPrevious = previousAllocations.TryGetValue(
                    sectionId,
                    out var previous);

                var updated = itemRequest.SectionAllocations
                    .FirstOrDefault(x => x.SectionId == sectionId);

                if (hasPrevious &&
                    updated != null &&
                    previous.Quantity == updated.Quantity)
                {
                    continue;
                }

                var updatedSection = updated == null
                    ? null
                    : sectionLookup[sectionId];

                historyItem.SectionAllocations.Add(
                    new PPMPChangeHistorySectionAllocation
                    {
                        PreviousSectionId = hasPrevious ? sectionId : null,
                        PreviousSectionCode = hasPrevious ? previous.SectionCode : null,
                        PreviousSectionName = hasPrevious ? previous.SectionName : null,
                        PreviousQuantity = hasPrevious ? previous.Quantity : null,

                        UpdatedSectionId = updated == null ? null : sectionId,
                        UpdatedSectionCode = updatedSection?.SectionCode,
                        UpdatedSectionName = updatedSection?.SectionName,
                        UpdatedQuantity = updated?.Quantity
                    });
            }

            history.Items.Add(historyItem);

            if (existingItem == null)
            {
                existingItem = new PPMPItem
                {
                    PPMPId = ppmp.PPMPId
                };

                ppmp.Items.Add(existingItem);
            }
            else
            {
                _context.PPMPSectionAllocations.RemoveRange(
                    existingItem.SectionAllocations.ToList());
            }

            existingItem.ItemId = itemRequest.ItemId;
            existingItem.CategoryId = itemRequest.CategoryId;

            existingItem.ItemCode = itemCode;
            existingItem.ItemDescription = description;
            existingItem.ItemType = itemType;
            existingItem.Unit = unit;
            existingItem.Specifications = specifications;

            existingItem.GeneralDescriptionAndObjective =
                generalDescriptionAndObjective;

            existingItem.ProjectType = projectType;

            existingItem.PreProcurementConference =
                itemRequest.PreProcurementConference;

            existingItem.Quantity = itemRequest.Quantity;
            existingItem.UnitPrice = itemRequest.UnitPrice;
            existingItem.EstimatedBudget = itemBudget;

            existingItem.SectionAllocations = itemRequest.SectionAllocations
                .Select(x => new PPMPSectionAllocation
                {
                    SectionId = x.SectionId,
                    Quantity = x.Quantity
                })
                .ToList();
        }

        var requestedItemIds = request.Items
            .Where(x => x.PPMPItemId.HasValue)
            .Select(x => x.PPMPItemId!.Value)
            .ToHashSet();

        var removedItems = originalItems
            .Where(x => !requestedItemIds.Contains(x.PPMPItemId))
            .ToList();

        foreach (var removedItem in removedItems)
        {
            changedItemCount++;

            var historyItem = new PPMPChangeHistoryItem
            {
                PreviousItemId = removedItem.ItemId,
                PreviousItemCode = removedItem.ItemCode,
                PreviousDescription = removedItem.ItemDescription,
                PreviousUnit = removedItem.Unit,
                PreviousQuantity = removedItem.Quantity,
                PreviousUnitPrice = removedItem.UnitPrice,
                PreviousEstimatedBudget = removedItem.EstimatedBudget,

                PreviousGeneralDescriptionAndObjective =
                    removedItem.GeneralDescriptionAndObjective,

                PreviousProjectType = removedItem.ProjectType,

                PreviousPreProcurementConference =
                    removedItem.PreProcurementConference
            };

            foreach (var allocation in removedItem.SectionAllocations)
            {
                historyItem.SectionAllocations.Add(
                    new PPMPChangeHistorySectionAllocation
                    {
                        PreviousSectionId = allocation.SectionId,
                        PreviousSectionCode = allocation.Section.SectionCode,
                        PreviousSectionName = allocation.Section.SectionName,
                        PreviousQuantity = allocation.Quantity
                    });
            }

            history.Items.Add(historyItem);

            _context.PPMPSectionAllocations.RemoveRange(
                removedItem.SectionAllocations);

            _context.PPMPItems.Remove(removedItem);
        }

        if (changedItemCount == 0)
        {
            throw new InvalidOperationException(
                "No changes were detected in the submitted PPMP.");
        }

        history.UpdatedBudget = newBudget;

        _context.PPMPChangeHistories.Add(history);

        ppmp.EstimatedBudget = newBudget;
        ppmp.Status = "Updated";
        ppmp.UpdatedAt = DateTime.UtcNow;

        // PPMP updates, removed allocations/items, and audit history
        // are persisted together in one EF Core SaveChanges transaction.
        await _context.SaveChangesAsync();

        var updatedPPMP = await GetByIdAsync(id, null);
        if (updatedPPMP == null)
        {
            throw new InvalidOperationException(
                "PPMP was updated, but its details could not be retrieved.");
        }

        return updatedPPMP;
    }
}
