namespace Drmc.Procurement.Application.Features.UserCategoryPreferences.DTO;

public class SaveUserCategoryPreferencesRequest
{
    public int PlanningYearId { get; set; }

    public List<int> PlanningYearCategoryIds { get; set; } = new();
}