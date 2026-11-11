namespace Drmc.Procurement.Application.Features.PlanningYearCategories.DTO;

public class PlanningYearCategoryDto
{
    public int PlanningYearCategoryId { get; set; }
    public int PlanningYearId { get; set; }
    public int Year { get; set; }
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
