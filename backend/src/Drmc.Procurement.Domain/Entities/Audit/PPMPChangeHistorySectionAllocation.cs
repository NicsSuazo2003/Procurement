namespace Drmc.Procurement.Domain.Entities.Audit;

public class PPMPChangeHistorySectionAllocation
{
    public int ChangeHistorySectionAllocationId { get; set; }

    public int ChangeHistoryItemId { get; set; }

    // Previous allocation snapshot
    public int? PreviousSectionId { get; set; }
    public string? PreviousSectionCode { get; set; }
    public string? PreviousSectionName { get; set; }
    public decimal? PreviousQuantity { get; set; }

    // Updated allocation snapshot
    public int? UpdatedSectionId { get; set; }
    public string? UpdatedSectionCode { get; set; }
    public string? UpdatedSectionName { get; set; }
    public decimal? UpdatedQuantity { get; set; }

    public PPMPChangeHistoryItem ChangeHistoryItem { get; set; } = null!;
}