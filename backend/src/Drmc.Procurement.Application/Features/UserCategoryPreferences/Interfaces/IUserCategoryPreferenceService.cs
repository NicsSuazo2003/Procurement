using Drmc.Procurement.Application.Features.UserCategoryPreferences.DTO;

namespace Drmc.Procurement.Application.Features.UserCategoryPreferences.Interfaces;

public interface IUserCategoryPreferenceService
{
    Task<List<UserCategoryPreferenceDto>> GetByUserAndPlanningYearAsync(
        int userId,
        int planningYearId);

    Task<List<UserCategoryPreferenceDto>> SaveAsync(
        int userId,
        SaveUserCategoryPreferencesRequest request);
}