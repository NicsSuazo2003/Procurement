using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Application.Features.PPMPs.Commands;

public sealed class CreatePPMPHandler(
    IPPMPRepository ppmpRepository,
    IPPMPReferenceRepository referenceRepository,
    IPPMPReadRepository readRepository)
    : IRequestHandler<CreatePPMPCommand, PPMPDetailDto>
{
    public async Task<PPMPDetailDto> Handle(
        CreatePPMPCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;

        if (request.DepartmentId != command.DepartmentId)
        {
            throw new InvalidOperationException(
                "You can only create a PPMP for your own department.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "A PPMP must contain at least one item.");
        }

        var department = await referenceRepository.GetActiveDepartmentAsync(
            request.DepartmentId,
            cancellationToken);
        if (department == null)
        {
            throw new InvalidOperationException(
                "The selected department does not exist or is inactive.");
        }

        var planningYear = await referenceRepository.GetPlanningYearAsync(
            request.PlanningYearId,
            cancellationToken);
        if (planningYear == null)
        {
            throw new InvalidOperationException(
                "The selected planning year does not exist.");
        }

        if (!string.Equals(planningYear.Status, "Open", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The selected planning year is not open.");
        }

        var creator = await referenceRepository.GetActiveUserAsync(
            command.CreatedBy,
            cancellationToken);
        if (creator == null)
        {
            throw new InvalidOperationException(
                "The selected creator does not exist or is inactive.");
        }

        if (creator.DepartmentId != command.DepartmentId)
        {
            throw new InvalidOperationException(
                "The creator does not belong to the selected department.");
        }

        var submittedCategoryIds = request.Items
            .Select(item => item.CategoryId)
            .Distinct()
            .ToHashSet();

        var validCategoryIds = await referenceRepository.GetActiveCategoryIdsAsync(
            request.PlanningYearId,
            submittedCategoryIds,
            cancellationToken);
        var invalidCategories = submittedCategoryIds
            .Except(validCategoryIds)
            .ToList();

        if (invalidCategories.Count > 0)
        {
            throw new InvalidOperationException(
                $"Category ID {invalidCategories[0]} is inactive or not assigned to the selected planning year.");
        }

        var submittedItemIds = request.Items
            .Where(item => item.ItemId.HasValue)
            .Select(item => item.ItemId!.Value)
            .Distinct()
            .ToHashSet();

        var catalogueItems = submittedItemIds.Count == 0
            ? new Dictionary<int, Item>()
            : await referenceRepository.GetActiveItemsByIdsAsync(
                submittedItemIds,
                cancellationToken);

        var missingItemIds = submittedItemIds
            .Except(catalogueItems.Keys)
            .ToList();
        if (missingItemIds.Count > 0)
        {
            throw new InvalidOperationException(
                $"Catalogue item ID {missingItemIds[0]} does not exist or is inactive.");
        }

        var submittedSectionIds = request.Items
            .SelectMany(item => item.SectionAllocations ?? new List<CreatePPMPSectionAllocationRequest>())
            .Select(allocation => allocation.SectionId)
            .Distinct()
            .ToHashSet();

        var validSectionIds = await referenceRepository.GetActiveDepartmentSectionIdsAsync(
            request.DepartmentId,
            submittedSectionIds,
            cancellationToken);
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
            PPMPReference = $"PPMP-{planningYear.Year}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            DepartmentId = request.DepartmentId,
            PlanningYearId = request.PlanningYearId,
            CreatedBy = command.CreatedBy,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            EstimatedBudget = 0
        };

        foreach (var itemRequest in request.Items)
        {
            var ppmpItem = BuildPPMPItem(itemRequest, catalogueItems);
            ppmp.Items.Add(ppmpItem);
            ppmp.EstimatedBudget += ppmpItem.EstimatedBudget;
        }

        await ppmpRepository.SaveNewAsync(ppmp, cancellationToken);

        var createdPPMP = await readRepository.GetByIdAsync(
            ppmp.PPMPId,
            command.DepartmentId,
            cancellationToken);
        if (createdPPMP == null)
        {
            throw new InvalidOperationException(
                "The PPMP was saved, but its details could not be retrieved.");
        }

        return createdPPMP;
    }

    private static PPMPItem BuildPPMPItem(
        CreatePPMPItemRequest itemRequest,
        IReadOnlyDictionary<int, Item> catalogueItems)
    {
        if (decimal.Round(itemRequest.Quantity, 2) != itemRequest.Quantity ||
            decimal.Round(itemRequest.UnitPrice, 2) != itemRequest.UnitPrice)
        {
            throw new InvalidOperationException(
                "Quantity and unit price must have no more than two decimal places.");
        }

        var itemId = itemRequest.ItemId;
        string itemCode;
        string itemDescription;
        string? itemType;
        string unit;
        string? specifications;

        if (itemId.HasValue)
        {
            var catalogueItem = catalogueItems[itemId.Value];
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

        var generalDescriptionAndObjective = itemRequest.GeneralDescriptionAndObjective.Trim();
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

        if (itemRequest.SectionAllocations
            .GroupBy(allocation => allocation.SectionId)
            .Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException(
                $"Item '{itemDescription}' contains duplicate section allocations.");
        }

        var allocationTotal = itemRequest.SectionAllocations.Sum(allocation => allocation.Quantity);
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
            GeneralDescriptionAndObjective = generalDescriptionAndObjective,
            ProjectType = projectType,
            PreProcurementConference = itemRequest.PreProcurementConference,
            Quantity = itemRequest.Quantity,
            UnitPrice = itemRequest.UnitPrice,
            EstimatedBudget = itemBudget
        };

        foreach (var allocation in itemRequest.SectionAllocations)
        {
            ppmpItem.SectionAllocations.Add(new PPMPSectionAllocation
            {
                SectionId = allocation.SectionId,
                Quantity = allocation.Quantity
            });
        }

        return ppmpItem;
    }
}
