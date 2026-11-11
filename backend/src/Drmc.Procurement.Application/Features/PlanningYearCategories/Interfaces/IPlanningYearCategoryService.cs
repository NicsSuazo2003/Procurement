using Drmc.Procurement.Application.Features.PlanningYearCategories.DTO;

namespace Drmc.Procurement.Application.Features.PlanningYearCategories.Interfaces;

public interface IPlanningYearCategoryService
{
    Task<List<PlanningYearCategoryDto>> GetAllAsync();

    Task<List<PlanningYearCategoryDto>> GetByPlanningYearIdAsync(int planningYearId);

    Task<PlanningYearCategoryDto?> GetByIdAsync(int id);

    Task<PlanningYearCategoryDto> AssignAsync(
        AssignPlanningYearCategoryRequest request);

    Task<bool> SetActiveStatusAsync(int id, bool isActive);
}
