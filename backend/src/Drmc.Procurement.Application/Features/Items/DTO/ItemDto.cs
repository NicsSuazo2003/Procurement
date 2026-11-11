namespace Drmc.Procurement.Application.Features.Items.DTO;

public class ItemDto
{
    public int ItemId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? ItemType { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Specifications { get; set; }
    public bool IsActive { get; set; }
}
