using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.Items.DTO;

public class UpdateItemRequest
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(300)]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ItemType { get; set; }

    [Required]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Specifications { get; set; }
}