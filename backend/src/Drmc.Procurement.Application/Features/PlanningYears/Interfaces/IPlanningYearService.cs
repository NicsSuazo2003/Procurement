using Drmc.Procurement.Application.Features.PlanningYears.DTO;

namespace Drmc.Procurement.Application.Features.PlanningYears.Interfaces;

public interface IPlanningYearService
{
    Task<List<PlanningYearDto>> GetAllAsync();

    Task<PlanningYearDto?> GetByIdAsync(int id);

    Task<PlanningYearDto> CreateAsync(CreatePlanningYearRequest request);

    Task<PlanningYearDto?> UpdateAsync(
        int id,
        CreatePlanningYearRequest request);

    Task<bool> UpdateStatusAsync(int id, string status);
}