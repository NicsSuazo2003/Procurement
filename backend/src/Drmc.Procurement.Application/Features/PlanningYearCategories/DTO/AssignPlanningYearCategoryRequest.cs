using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.PlanningYearCategories.DTO;

public class AssignPlanningYearCategoryRequest
{
    [Range(1, int.MaxValue)]
    public int PlanningYearId { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}
