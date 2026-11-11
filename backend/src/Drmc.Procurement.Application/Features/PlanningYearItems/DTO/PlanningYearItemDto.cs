
namespace Drmc.Procurement.Application.Features.PlanningYearItems.DTO;

public class PlanningYearItemDto
{
    public int PlanningYearItemId { get; set; }

    public int PlanningYearCategoryId { get; set; }
    public int PlanningYearId { get; set; }
    public int PlanningYear { get; set; }

    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Specifications { get; set; }

    public decimal ABCPerUnit { get; set; }

    public bool IsActive { get; set; }
}
