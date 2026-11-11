
namespace Drmc.Procurement.Domain.Entities.Audit;

public class PPMPChangeHistoryItem
{
    public int ChangeHistoryItemId { get; set; }

    public int ChangeHistoryId { get; set; }

    // Original item information
    public int? PreviousItemId { get; set; }
    public string? PreviousItemCode { get; set; }
    public string? PreviousDescription { get; set; }
    public string? PreviousUnit { get; set; }
    public decimal? PreviousQuantity { get; set; }
    public decimal? PreviousUnitPrice { get; set; }
    public decimal? PreviousEstimatedBudget { get; set; }

    // Original official PPMP form fields
    public string? PreviousGeneralDescriptionAndObjective { get; set; }
    public string? PreviousProjectType { get; set; }
    public bool? PreviousPreProcurementConference { get; set; }

    // Updated item information
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

    public PPMPChangeHistory ChangeHistory { get; set; } = null!;

    public ICollection<PPMPChangeHistorySectionAllocation> SectionAllocations
    { get; set; } = new List<PPMPChangeHistorySectionAllocation>();
}
