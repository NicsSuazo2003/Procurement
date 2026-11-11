namespace Drmc.Procurement.Domain.Entities.Identity;

using Drmc.Procurement.Domain.Entities.Planning;

public class UserCategoryPreference
{
    public int UserCategoryPreferenceId { get; set; }

    public int UserId { get; set; }

    public int PlanningYearCategoryId { get; set; }

    public User User { get; set; } = null!;

    public PlanningYearCategory PlanningYearCategory { get; set; } = null!;
}