
using Drmc.Procurement.Application.Features.PlanningYearItems.DTO;

namespace Drmc.Procurement.Application.Features.PlanningYearItems.Interfaces;

public interface IPlanningYearItemService
{
    Task<List<PlanningYearItemDto>> GetAllAsync();

    Task<List<PlanningYearItemDto>> GetByPlanningYearCategoryIdAsync(
        int planningYearCategoryId);

    Task<PlanningYearItemDto?> GetByIdAsync(int id);

    Task<PlanningYearItemDto> AssignAsync(
        AssignPlanningYearItemRequest request);

    Task<PlanningYearItemDto?> UpdateAsync(
        int id,
        UpdatePlanningYearItemRequest request);

    Task<bool> SetActiveStatusAsync(int id, bool isActive);
}
