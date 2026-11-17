
using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class CreatePPMPItemRequest
{
    public int? ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    // Optional internal/catalogue information
    [StringLength(100)]
    public string ItemCode { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string ItemDescription { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ItemType { get; set; }

    [Required]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Specifications { get; set; }

    // Official PPMP form fields
    [Required]
    [StringLength(2000)]
    public string GeneralDescriptionAndObjective { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ProjectType { get; set; } = string.Empty;

    public bool PreProcurementConference { get; set; }

    [StringLength(150)]
    public string RecommendedModeOfProcurement { get; set; } = string.Empty;

    [StringLength(7)]
    public string StartOfProcurementActivity { get; set; } = string.Empty;

    [StringLength(7)]
    public string EndOfProcurementActivity { get; set; } = string.Empty;

    [StringLength(100)]
    public string ExpectedDeliveryImplementationPeriod { get; set; } = string.Empty;

    [StringLength(100)]
    public string SourceOfFunds { get; set; } = string.Empty;

    [StringLength(500)]
    public string AttachedSupportingDocuments { get; set; } = string.Empty;

    // Quantity and ABC
    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Quantity { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999")]
    public decimal UnitPrice { get; set; }

    public List<CreatePPMPSectionAllocationRequest> SectionAllocations { get; set; }
        = new();
}
