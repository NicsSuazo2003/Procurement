using Drmc.Procurement.Application.Features.DepartmentSections.DTO;

namespace Drmc.Procurement.Application.Features.DepartmentSections.Interfaces;

public interface IDepartmentSectionService
{
    Task<List<DepartmentSectionDto>> GetAllAsync();

    Task<List<DepartmentSectionDto>> GetByDepartmentIdAsync(
        int departmentId);

    Task<DepartmentSectionDto?> GetByIdAsync(int id);

    Task<DepartmentSectionDto> CreateAsync(
        CreateDepartmentSectionRequest request);

    Task<DepartmentSectionDto?> UpdateAsync(
        int id,
        CreateDepartmentSectionRequest request);

    Task<bool> SetActiveStatusAsync(int id, bool isActive);
}