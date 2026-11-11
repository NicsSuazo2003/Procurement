namespace Drmc.Procurement.Application.Features.UserCategoryPreferences.DTO;

public class UserCategoryPreferenceDto
{
    public int PlanningYearCategoryId { get; set; }

    public int PlanningYearId { get; set; }

    public int CategoryId { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;
}