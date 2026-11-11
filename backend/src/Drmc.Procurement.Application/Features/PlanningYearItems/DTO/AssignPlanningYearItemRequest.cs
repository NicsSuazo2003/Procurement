
namespace Drmc.Procurement.Application.Features.PlanningYearItems.DTO;

public class AssignPlanningYearItemRequest
{
    public int PlanningYearCategoryId { get; set; }

    public int ItemId { get; set; }

    public decimal ABCPerUnit { get; set; }
}
