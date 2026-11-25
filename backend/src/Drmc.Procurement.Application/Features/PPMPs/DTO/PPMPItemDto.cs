
namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class PPMPItemDto
{
    public int PPMPItemId { get; set; }

    public int? ItemId { get; set; }

    public int CategoryId { get; set; }
    public int? TeamId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    // Internal/catalogue information
    public string ItemCode { get; set; } = string.Empty;

    public string ItemDescription { get; set; } = string.Empty;

    public string? ItemType { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string? Specifications { get; set; }

    // Official PPMP form fields
    public string GeneralDescriptionAndObjective { get; set; } = string.Empty;

    public string ProjectType { get; set; } = string.Empty;

    public bool PreProcurementConference { get; set; }

    public string RecommendedModeOfProcurement { get; set; } = string.Empty;

    public string StartOfProcurementActivity { get; set; } = string.Empty;

    public string EndOfProcurementActivity { get; set; } = string.Empty;

    public string ExpectedDeliveryImplementationPeriod { get; set; } = string.Empty;

    public string SourceOfFunds { get; set; } = string.Empty;

    public string AttachedSupportingDocuments { get; set; } = string.Empty;

    // Quantity and ABC
    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal EstimatedBudget { get; set; }

    public List<PPMPSectionAllocationDto> SectionAllocations { get; set; } = new();
}
