using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Domain.Entities.APP;

public class APPItem
{
    public int APPItemId { get; set; }

    public int APPCategoryId { get; set; }
    public int? ItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string? ItemType { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Specifications { get; set; }

    public decimal TotalQuantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedBudget { get; set; }

    public APPCategory APPCategory { get; set; } = null!;
    public Item? Item { get; set; }

    public ICollection<APPItemSource> Sources { get; set; }
        = new List<APPItemSource>();
}