namespace Drmc.Procurement.Domain.Entities.Procurement;

public class Item
{
    public int ItemId { get; set; }

    public int CategoryId { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string? ItemType { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string? Specifications { get; set; }

    public bool IsActive { get; set; } = true;

    public ProcurementCategory Category { get; set; } = null!;
}