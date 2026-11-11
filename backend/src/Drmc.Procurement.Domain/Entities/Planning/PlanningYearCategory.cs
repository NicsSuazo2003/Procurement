using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Procurement;
using Drmc.Procurement.Domain.Entities.Identity;

namespace Drmc.Procurement.Domain.Entities.Planning;

public class PlanningYearCategory
{
    public int PlanningYearCategoryId { get; set; }

    public int PlanningYearId { get; set; }

    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    public PlanningYear PlanningYear { get; set; } = null!;

    public ProcurementCategory Category { get; set; } = null!;
    public ICollection<PlanningYearItem> Items { get; set; }
    = new List<PlanningYearItem>();

    public ICollection<UserCategoryPreference> UserPreferences { get; set; }
    = new List<UserCategoryPreference>();
}