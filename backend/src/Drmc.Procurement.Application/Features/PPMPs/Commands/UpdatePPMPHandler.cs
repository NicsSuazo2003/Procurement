using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Audit;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Application.Features.PPMPs.Commands;

public sealed class UpdatePPMPHandler(
    IPPMPRepository ppmpRepository,
    IPPMPReferenceRepository referenceRepository,
    IPPMPReadRepository readRepository)
    : IRequestHandler<UpdatePPMPCommand, PPMPDetailDto>
{
    public async Task<PPMPDetailDto> Handle(
        UpdatePPMPCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;
        var ppmp = await ppmpRepository.GetForUpdateAsync(command.Id, cancellationToken);
        if (ppmp == null)
        {
            throw new InvalidOperationException($"PPMP with ID {command.Id} was not found.");
        }

        if (ppmp.Status != "Under Review")
        {
            throw new InvalidOperationException("Only PPMPs under Procurement review can be updated.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            throw new InvalidOperationException("A PPMP must contain at least one item.");
        }

        if (await referenceRepository.GetActiveUserAsync(command.ChangedBy, cancellationToken) == null)
        {
            throw new InvalidOperationException("The selected user does not exist or is inactive.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException("A reason for the changes is required.");
        }

        var reason = request.Reason.Trim();
        if (reason.Length > 1000)
        {
            throw new InvalidOperationException("The change reason cannot exceed 1000 characters.");
        }

        if (request.Items
            .Where(item => item.PPMPItemId.HasValue)
            .GroupBy(item => item.PPMPItemId)
            .Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException("The update contains duplicate PPMP item IDs.");
        }

        var originalItems = ppmp.Items.ToList();
        var existingItemIds = originalItems.Select(item => item.PPMPItemId).ToHashSet();
        if (request.Items.Any(item =>
                item.PPMPItemId.HasValue && !existingItemIds.Contains(item.PPMPItemId.Value)))
        {
            throw new InvalidOperationException("One or more PPMP item IDs do not belong to this PPMP.");
        }

        var submittedCategoryIds = request.Items.Select(item => item.CategoryId).Distinct().ToHashSet();
        var validCategoryIds = await referenceRepository.GetActiveCategoryIdsAsync(
            ppmp.PlanningYearId,
            submittedCategoryIds,
            cancellationToken);
        var invalidCategories = submittedCategoryIds.Except(validCategoryIds).ToList();
        if (invalidCategories.Count > 0)
        {
            throw new InvalidOperationException(
                $"Category ID {invalidCategories[0]} is inactive or not assigned to this planning year.");
        }

        var submittedItemIds = request.Items
            .Where(item => item.ItemId.HasValue)
            .Select(item => item.ItemId!.Value)
            .Distinct()
            .ToHashSet();
        var catalogueItems = submittedItemIds.Count == 0
            ? new Dictionary<int, Domain.Entities.Procurement.Item>()
            : await referenceRepository.GetActiveItemsByIdsAsync(submittedItemIds, cancellationToken);
        var missingItemIds = submittedItemIds.Except(catalogueItems.Keys).ToList();
        if (missingItemIds.Count > 0)
        {
            throw new InvalidOperationException(
                $"Catalogue item ID {missingItemIds[0]} does not exist or is inactive.");
        }

        var submittedSectionIds = request.Items
            .SelectMany(item => item.SectionAllocations ?? [])
            .Select(allocation => allocation.SectionId)
            .Distinct()
            .ToHashSet();
        var validSectionIds = await referenceRepository.GetActiveDepartmentSectionIdsAsync(
            ppmp.DepartmentId,
            submittedSectionIds,
            cancellationToken);
        var invalidSections = submittedSectionIds.Except(validSectionIds).ToList();
        if (invalidSections.Count > 0)
        {
            throw new InvalidOperationException($"Section ID {invalidSections[0]} is invalid or inactive.");
        }

        var sectionLookup = await referenceRepository.GetActiveDepartmentSectionsAsync(
            ppmp.DepartmentId,
            validSectionIds,
            cancellationToken);

        var oldBudget = ppmp.EstimatedBudget;
        decimal newBudget = 0;
        var history = new PPMPChangeHistory
        {
            PPMPId = ppmp.PPMPId,
            ChangedBy = command.ChangedBy,
            Reason = reason,
            PreviousBudget = oldBudget,
            ChangedAt = DateTime.UtcNow
        };
        var changedItemCount = 0;
        var removedAllocations = new List<PPMPSectionAllocation>();

        foreach (var itemRequest in request.Items)
        {
            var updatedItem = BuildUpdatedItem(itemRequest, catalogueItems);
            newBudget += updatedItem.EstimatedBudget;

            var existingItem = itemRequest.PPMPItemId.HasValue
                ? originalItems.Single(item => item.PPMPItemId == itemRequest.PPMPItemId.Value)
                : null;
            var previousAllocations = existingItem?.SectionAllocations.ToDictionary(
                allocation => allocation.SectionId,
                allocation => (
                    allocation.Quantity,
                    allocation.Section.SectionCode,
                    allocation.Section.SectionName))
                ?? new Dictionary<int, (decimal Quantity, string SectionCode, string SectionName)>();

            var itemChanged = existingItem == null || HasItemChanged(existingItem, updatedItem);
            var allocationChanged = existingItem != null &&
                (existingItem.SectionAllocations.Count != itemRequest.SectionAllocations.Count ||
                 itemRequest.SectionAllocations.Any(allocation =>
                     !previousAllocations.TryGetValue(allocation.SectionId, out var previous) ||
                     previous.Quantity != allocation.Quantity));

            if (!itemChanged && !allocationChanged)
            {
                continue;
            }

            changedItemCount++;
            var historyItem = CreateHistoryItem(existingItem, updatedItem);
            AddAllocationSnapshots(historyItem, previousAllocations, itemRequest.SectionAllocations, sectionLookup);
            history.Items.Add(historyItem);

            if (existingItem == null)
            {
                existingItem = new PPMPItem { PPMPId = ppmp.PPMPId };
                ppmp.Items.Add(existingItem);
            }
            else
            {
                removedAllocations.AddRange(existingItem.SectionAllocations);
            }

            CopyItemValues(updatedItem, existingItem);
            existingItem.SectionAllocations = itemRequest.SectionAllocations
                .Select(allocation => new PPMPSectionAllocation
                {
                    SectionId = allocation.SectionId,
                    Quantity = allocation.Quantity
                })
                .ToList();
        }

        var requestedItemIds = request.Items
            .Where(item => item.PPMPItemId.HasValue)
            .Select(item => item.PPMPItemId!.Value)
            .ToHashSet();
        var removedItems = originalItems
            .Where(item => !requestedItemIds.Contains(item.PPMPItemId))
            .ToList();
        foreach (var removedItem in removedItems)
        {
            changedItemCount++;
            var historyItem = CreateHistoryItem(removedItem, null);
            foreach (var allocation in removedItem.SectionAllocations)
            {
                historyItem.SectionAllocations.Add(new PPMPChangeHistorySectionAllocation
                {
                    PreviousSectionId = allocation.SectionId,
                    PreviousSectionCode = allocation.Section.SectionCode,
                    PreviousSectionName = allocation.Section.SectionName,
                    PreviousQuantity = allocation.Quantity
                });
            }

            history.Items.Add(historyItem);
            removedAllocations.AddRange(removedItem.SectionAllocations);
        }

        if (changedItemCount == 0)
        {
            throw new InvalidOperationException("No changes were detected in the submitted PPMP.");
        }

        history.UpdatedBudget = newBudget;
        ppmp.EstimatedBudget = newBudget;
        ppmp.Status = "Updated";
        ppmp.UpdatedAt = DateTime.UtcNow;

        await ppmpRepository.SaveUpdateAsync(
            ppmp,
            removedAllocations,
            removedItems,
            history,
            cancellationToken);

        var result = await readRepository.GetByIdAsync(command.Id, null, cancellationToken);
        if (result == null)
        {
            throw new InvalidOperationException("PPMP was updated, but its details could not be retrieved.");
        }

        return result;
    }

    private static PPMPItem BuildUpdatedItem(
        UpdatePPMPItemRequest request,
        IReadOnlyDictionary<int, Domain.Entities.Procurement.Item> catalogueItems)
    {
        if (request.Quantity <= 0 || request.UnitPrice < 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero and unit price cannot be negative.");
        }

        if (decimal.Round(request.Quantity, 2) != request.Quantity ||
            decimal.Round(request.UnitPrice, 2) != request.UnitPrice)
        {
            throw new InvalidOperationException("Quantity and unit price must have no more than two decimal places.");
        }

        string itemCode;
        string description;
        string? itemType;
        string unit;
        string? specifications;
        if (request.ItemId.HasValue)
        {
            var catalogueItem = catalogueItems[request.ItemId.Value];
            if (catalogueItem.CategoryId != request.CategoryId)
            {
                throw new InvalidOperationException("The catalogue item does not belong to the selected category.");
            }

            itemCode = catalogueItem.ItemCode;
            description = catalogueItem.ItemName;
            itemType = catalogueItem.ItemType;
            unit = catalogueItem.Unit;
            specifications = catalogueItem.Specifications;
        }
        else
        {
            itemCode = request.ItemCode?.Trim() ?? string.Empty;
            description = request.ItemDescription.Trim();
            itemType = request.ItemType?.Trim();
            unit = request.Unit.Trim();
            specifications = request.Specifications?.Trim();
        }

        var generalDescription = request.GeneralDescriptionAndObjective.Trim();
        var projectType = request.ProjectType.Trim();
        if (string.IsNullOrWhiteSpace(generalDescription))
        {
            throw new InvalidOperationException("General description and objective are required.");
        }

        if (string.IsNullOrWhiteSpace(projectType))
        {
            throw new InvalidOperationException("Project type is required.");
        }

        if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(unit))
        {
            throw new InvalidOperationException("Item description and unit are required.");
        }

        if (request.SectionAllocations == null || request.SectionAllocations.Count == 0)
        {
            throw new InvalidOperationException($"Item '{description}' must have at least one section allocation.");
        }

        if (request.SectionAllocations.GroupBy(allocation => allocation.SectionId).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException($"Item '{description}' contains duplicate section allocations.");
        }

        if (request.SectionAllocations.Any(allocation => allocation.Quantity <= 0))
        {
            throw new InvalidOperationException("Section allocation quantities must be greater than zero.");
        }

        if (request.SectionAllocations.Any(allocation => decimal.Round(allocation.Quantity, 2) != allocation.Quantity))
        {
            throw new InvalidOperationException("Section allocation quantities must have no more than two decimal places.");
        }

        if (request.SectionAllocations.Sum(allocation => allocation.Quantity) != request.Quantity)
        {
            throw new InvalidOperationException($"Section allocations for '{description}' must equal the total item quantity.");
        }

        return new PPMPItem
        {
            ItemId = request.ItemId,
            CategoryId = request.CategoryId,
            ItemCode = itemCode,
            ItemDescription = description,
            ItemType = itemType,
            Unit = unit,
            Specifications = specifications,
            GeneralDescriptionAndObjective = generalDescription,
            ProjectType = projectType,
            PreProcurementConference = request.PreProcurementConference,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            EstimatedBudget = request.Quantity * request.UnitPrice
        };
    }

    private static bool HasItemChanged(PPMPItem current, PPMPItem updated) =>
        current.ItemId != updated.ItemId ||
        current.CategoryId != updated.CategoryId ||
        current.ItemCode != updated.ItemCode ||
        current.ItemDescription != updated.ItemDescription ||
        current.ItemType != updated.ItemType ||
        current.Unit != updated.Unit ||
        current.Specifications != updated.Specifications ||
        current.GeneralDescriptionAndObjective != updated.GeneralDescriptionAndObjective ||
        current.ProjectType != updated.ProjectType ||
        current.PreProcurementConference != updated.PreProcurementConference ||
        current.Quantity != updated.Quantity ||
        current.UnitPrice != updated.UnitPrice ||
        current.EstimatedBudget != updated.EstimatedBudget;

    private static PPMPChangeHistoryItem CreateHistoryItem(PPMPItem? previous, PPMPItem? updated) =>
        new()
        {
            PreviousItemId = previous?.ItemId,
            PreviousItemCode = previous?.ItemCode,
            PreviousDescription = previous?.ItemDescription,
            PreviousUnit = previous?.Unit,
            PreviousQuantity = previous?.Quantity,
            PreviousUnitPrice = previous?.UnitPrice,
            PreviousEstimatedBudget = previous?.EstimatedBudget,
            PreviousGeneralDescriptionAndObjective = previous?.GeneralDescriptionAndObjective,
            PreviousProjectType = previous?.ProjectType,
            PreviousPreProcurementConference = previous?.PreProcurementConference,
            UpdatedItemId = updated?.ItemId,
            UpdatedItemCode = updated?.ItemCode,
            UpdatedDescription = updated?.ItemDescription,
            UpdatedUnit = updated?.Unit,
            UpdatedQuantity = updated?.Quantity,
            UpdatedUnitPrice = updated?.UnitPrice,
            UpdatedEstimatedBudget = updated?.EstimatedBudget,
            UpdatedGeneralDescriptionAndObjective = updated?.GeneralDescriptionAndObjective,
            UpdatedProjectType = updated?.ProjectType,
            UpdatedPreProcurementConference = updated?.PreProcurementConference
        };

    private static void AddAllocationSnapshots(
        PPMPChangeHistoryItem historyItem,
        IReadOnlyDictionary<int, (decimal Quantity, string SectionCode, string SectionName)> previousAllocations,
        IReadOnlyCollection<UpdatePPMPSectionAllocationRequest> updatedAllocations,
        IReadOnlyDictionary<int, Drmc.Procurement.Domain.Entities.Organization.DepartmentSection> sections)
    {
        var updatedBySection = updatedAllocations.ToDictionary(allocation => allocation.SectionId);
        foreach (var sectionId in previousAllocations.Keys.Union(updatedBySection.Keys))
        {
            var hasPrevious = previousAllocations.TryGetValue(sectionId, out var previous);
            var hasUpdated = updatedBySection.TryGetValue(sectionId, out var updated);
            if (hasPrevious && hasUpdated && previous.Quantity == updated!.Quantity)
            {
                continue;
            }

            var section = hasUpdated ? sections[sectionId] : null;
            historyItem.SectionAllocations.Add(new PPMPChangeHistorySectionAllocation
            {
                PreviousSectionId = hasPrevious ? sectionId : null,
                PreviousSectionCode = hasPrevious ? previous.SectionCode : null,
                PreviousSectionName = hasPrevious ? previous.SectionName : null,
                PreviousQuantity = hasPrevious ? previous.Quantity : null,
                UpdatedSectionId = hasUpdated ? sectionId : null,
                UpdatedSectionCode = section?.SectionCode,
                UpdatedSectionName = section?.SectionName,
                UpdatedQuantity = updated?.Quantity
            });
        }
    }

    private static void CopyItemValues(PPMPItem source, PPMPItem target)
    {
        target.ItemId = source.ItemId;
        target.CategoryId = source.CategoryId;
        target.ItemCode = source.ItemCode;
        target.ItemDescription = source.ItemDescription;
        target.ItemType = source.ItemType;
        target.Unit = source.Unit;
        target.Specifications = source.Specifications;
        target.GeneralDescriptionAndObjective = source.GeneralDescriptionAndObjective;
        target.ProjectType = source.ProjectType;
        target.PreProcurementConference = source.PreProcurementConference;
        target.Quantity = source.Quantity;
        target.UnitPrice = source.UnitPrice;
        target.EstimatedBudget = source.EstimatedBudget;
    }
}
