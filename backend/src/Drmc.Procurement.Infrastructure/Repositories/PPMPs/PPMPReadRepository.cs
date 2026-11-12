using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Procurement;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Repositories.PPMPs;

public sealed class PPMPReadRepository(ApplicationDbContext context) : IPPMPReadRepository
{
    public async Task<PagedResult<PPMPDto>> GetPagedAsync(
        int? departmentId,
        int page,
        int pageSize,
        string[]? statuses,
        int? planningYear,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 25;
        if (pageSize > 100) pageSize = 100;

        var query = context.PPMPs.AsNoTracking().AsQueryable();
        if (departmentId.HasValue)
        {
            query = query.Where(ppmp => ppmp.DepartmentId == departmentId.Value);
        }

        if (planningYear.HasValue)
        {
            query = query.Where(ppmp => ppmp.PlanningYear.Year == planningYear.Value);
        }

        var normalizedStatuses = (statuses ?? Array.Empty<string>())
            .Where(status => !string.IsNullOrWhiteSpace(status))
            .SelectMany(status => status.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(status => status.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (normalizedStatuses.Length > 0)
        {
            query = query.Where(ppmp => normalizedStatuses.Contains(ppmp.Status));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await ProjectList(query)
            .OrderByDescending(ppmp => ppmp.SubmittedAt)
            .ThenByDescending(ppmp => ppmp.PPMPId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PPMPDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PPPMSummaryDto> GetSummaryAsync(
        int? departmentId,
        CancellationToken cancellationToken = default)
    {
        var query = context.PPMPs.AsNoTracking().AsQueryable();
        if (departmentId.HasValue)
        {
            query = query.Where(ppmp => ppmp.DepartmentId == departmentId.Value);
        }

        var countsByStatus = await query
            .GroupBy(ppmp => ppmp.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var byStatus = countsByStatus.ToDictionary(
            item => item.Status,
            item => item.Count,
            StringComparer.OrdinalIgnoreCase);

        static int Get(Dictionary<string, int> statuses, string status) =>
            statuses.TryGetValue(status, out var count) ? count : 0;

        return new PPPMSummaryDto
        {
            Total = byStatus.Values.Sum(),
            AwaitingAction =
                Get(byStatus, "For Unit Head Approval") +
                Get(byStatus, "Approved by Unit Head") +
                Get(byStatus, "Under Review"),
            CompletedReview =
                Get(byStatus, "Reviewed") +
                Get(byStatus, "Included in APP"),
            ByStatus = byStatus
        };
    }

    private static IQueryable<PPMPDto> ProjectList(IQueryable<PPMP> query) =>
        query.Select(ppmp => new PPMPDto
        {
            PPMPId = ppmp.PPMPId,
            PPMPReference = ppmp.PPMPReference,
            DepartmentId = ppmp.DepartmentId,
            DepartmentName = ppmp.Department.DepartmentName,
            PlanningYearId = ppmp.PlanningYearId,
            PlanningYear = ppmp.PlanningYear.Year,
            CreatedBy = ppmp.CreatedBy,
            CreatedByName = ppmp.Creator.FullName,
            Status = ppmp.Status,
            EstimatedBudget = ppmp.EstimatedBudget,
            CreatedAt = ppmp.CreatedAt,
            SubmittedAt = ppmp.SubmittedAt,
            UpdatedAt = ppmp.UpdatedAt,
            ItemCount = ppmp.Items.Count()
        });

    public Task<PPMPDetailDto?> GetByIdAsync(
        int id,
        int? departmentId,
        CancellationToken cancellationToken = default) =>
        context.PPMPs
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ppmp =>
                ppmp.PPMPId == id &&
                (!departmentId.HasValue || ppmp.DepartmentId == departmentId.Value))
            .Select(ppmp => new PPMPDetailDto
            {
                PPMPId = ppmp.PPMPId,
                PPMPReference = ppmp.PPMPReference,
                DepartmentId = ppmp.DepartmentId,
                DepartmentName = ppmp.Department.DepartmentName,
                PlanningYearId = ppmp.PlanningYearId,
                PlanningYear = ppmp.PlanningYear.Year,
                CreatedBy = ppmp.CreatedBy,
                CreatedByName = ppmp.Creator.FullName,
                Status = ppmp.Status,
                EstimatedBudget = ppmp.EstimatedBudget,
                CreatedAt = ppmp.CreatedAt,
                SubmittedAt = ppmp.SubmittedAt,
                UpdatedAt = ppmp.UpdatedAt,
                Items = ppmp.Items
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
                        GeneralDescriptionAndObjective = item.GeneralDescriptionAndObjective,
                        ProjectType = item.ProjectType,
                        PreProcurementConference = item.PreProcurementConference,
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
            .FirstOrDefaultAsync(cancellationToken);

    public Task<List<PPMPChangeHistoryDto>> GetChangeHistoryAsync(
        int id,
        int? departmentId,
        CancellationToken cancellationToken = default) =>
        context.PPMPChangeHistories
            .AsNoTracking()
            .AsSplitQuery()
            .Where(history =>
                history.PPMPId == id &&
                (!departmentId.HasValue || history.PPMP.DepartmentId == departmentId.Value))
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
                        PreviousGeneralDescriptionAndObjective = item.PreviousGeneralDescriptionAndObjective,
                        PreviousProjectType = item.PreviousProjectType,
                        PreviousPreProcurementConference = item.PreviousPreProcurementConference,
                        UpdatedItemId = item.UpdatedItemId,
                        UpdatedItemCode = item.UpdatedItemCode,
                        UpdatedDescription = item.UpdatedDescription,
                        UpdatedUnit = item.UpdatedUnit,
                        UpdatedQuantity = item.UpdatedQuantity,
                        UpdatedUnitPrice = item.UpdatedUnitPrice,
                        UpdatedEstimatedBudget = item.UpdatedEstimatedBudget,
                        UpdatedGeneralDescriptionAndObjective = item.UpdatedGeneralDescriptionAndObjective,
                        UpdatedProjectType = item.UpdatedProjectType,
                        UpdatedPreProcurementConference = item.UpdatedPreProcurementConference,
                        SectionAllocations = item.SectionAllocations
                            .Select(allocation => new PPMPChangeHistorySectionAllocationDto
                            {
                                ChangeHistorySectionAllocationId = allocation.ChangeHistorySectionAllocationId,
                                PreviousSectionId = allocation.PreviousSectionId,
                                PreviousSectionCode = allocation.PreviousSectionCode,
                                PreviousSectionName = allocation.PreviousSectionName,
                                PreviousQuantity = allocation.PreviousQuantity,
                                UpdatedSectionId = allocation.UpdatedSectionId,
                                UpdatedSectionCode = allocation.UpdatedSectionCode,
                                UpdatedSectionName = allocation.UpdatedSectionName,
                                UpdatedQuantity = allocation.UpdatedQuantity
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
}
