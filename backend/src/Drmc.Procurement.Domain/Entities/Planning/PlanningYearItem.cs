
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Domain.Entities.Planning;

public class PlanningYearItem
{
    public int PlanningYearItemId { get; set; }

    public int PlanningYearCategoryId { get; set; }

    public int ItemId { get; set; }

    public decimal ABCPerUnit { get; set; }

    public bool IsActive { get; set; } = true;

    public PlanningYearCategory PlanningYearCategory { get; set; } = null!;

    public Item Item { get; set; } = null!;
}
