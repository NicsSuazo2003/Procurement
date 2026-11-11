using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.ProcurementCategories.DTO;

public class CreateProcurementCategoryRequest
{
    [Required]
    [StringLength(30)]
    public string CategoryCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}