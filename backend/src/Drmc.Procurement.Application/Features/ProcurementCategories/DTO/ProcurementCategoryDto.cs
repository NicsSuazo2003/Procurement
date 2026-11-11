namespace Drmc.Procurement.Application.Features.ProcurementCategories.DTO;

public class ProcurementCategoryDto
{
    public int CategoryId { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}