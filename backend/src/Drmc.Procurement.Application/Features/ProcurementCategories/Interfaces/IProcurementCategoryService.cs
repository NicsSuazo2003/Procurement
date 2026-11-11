using Drmc.Procurement.Application.Features.ProcurementCategories.DTO;

namespace Drmc.Procurement.Application.Features.ProcurementCategories.Interfaces;

public interface IProcurementCategoryService
{
    Task<List<ProcurementCategoryDto>> GetAllAsync();

    Task<ProcurementCategoryDto?> GetByIdAsync(int id);

    Task<List<ProcurementCategoryAuditLogDto>> GetHistoryAsync(int id);

    Task<ProcurementCategoryDto> CreateAsync(
        CreateProcurementCategoryRequest request,
        int changedByUserId);

    Task<ProcurementCategoryDto?> UpdateAsync(
        int id,
        CreateProcurementCategoryRequest request,
        int changedByUserId);

    Task<bool> SetActiveStatusAsync(
        int id,
        bool isActive,
        int changedByUserId);
}