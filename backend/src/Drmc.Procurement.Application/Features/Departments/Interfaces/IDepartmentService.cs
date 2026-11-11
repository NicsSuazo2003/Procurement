
using Drmc.Procurement.Application.Features.Departments.DTO;

namespace Drmc.Procurement.Application.Features.Departments.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();

    Task<DepartmentDto?> GetByIdAsync(int id);

    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request);

    Task<DepartmentDto?> UpdateAsync(
        int id,
        CreateDepartmentRequest request);

    Task<bool> SetActiveStatusAsync(int id, bool isActive);
}
