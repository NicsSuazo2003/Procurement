
namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class PPMPChangeHistoryDto
{
    public int ChangeHistoryId { get; set; }
    public int PPMPId { get; set; }

    public int ChangedBy { get; set; }
    public string ChangedByName { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public decimal PreviousBudget { get; set; }
    public decimal UpdatedBudget { get; set; }

    public DateTime ChangedAt { get; set; }

    public List<PPMPChangeHistoryItemDto> Items { get; set; } = new();
}

public class PPMPChangeHistoryItemDto
{
    public int ChangeHistoryItemId { get; set; }

    // Previous item
    public int? PreviousItemId { get; set; }
    public string? PreviousItemCode { get; set; }
    public string? PreviousDescription { get; set; }
    public string? PreviousUnit { get; set; }
    public decimal? PreviousQuantity { get; set; }
    public decimal? PreviousUnitPrice { get; set; }
    public decimal? PreviousEstimatedBudget { get; set; }

    // Previous official PPMP form fields
    public string? PreviousGeneralDescriptionAndObjective { get; set; }
    public string? PreviousProjectType { get; set; }
    public bool? PreviousPreProcurementConference { get; set; }

    // Updated item
    public int? UpdatedItemId { get; set; }
    public string? UpdatedItemCode { get; set; }
    public string? UpdatedDescription { get; set; }
    public string? UpdatedUnit { get; set; }
    public decimal? UpdatedQuantity { get; set; }
    public decimal? UpdatedUnitPrice { get; set; }
    public decimal? UpdatedEstimatedBudget { get; set; }

    // Updated official PPMP form fields
    public string? UpdatedGeneralDescriptionAndObjective { get; set; }
    public string? UpdatedProjectType { get; set; }
    public bool? UpdatedPreProcurementConference { get; set; }

    public List<PPMPChangeHistorySectionAllocationDto>
        SectionAllocations { get; set; } = new();
}

public class PPMPChangeHistorySectionAllocationDto
{
    public int ChangeHistorySectionAllocationId { get; set; }

    // Previous allocation
    public int? PreviousSectionId { get; set; }
    public string? PreviousSectionCode { get; set; }
    public string? PreviousSectionName { get; set; }
    public decimal? PreviousQuantity { get; set; }

    // Updated allocation
    public int? UpdatedSectionId { get; set; }
    public string? UpdatedSectionCode { get; set; }
    public string? UpdatedSectionName { get; set; }
    public decimal? UpdatedQuantity { get; set; }
}
