using Drmc.Procurement.Application.Features.Roles.DTO;

namespace Drmc.Procurement.Application.Features.Roles.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync();

    Task<RoleDto?> GetByIdAsync(int id);

    Task<RoleDto> CreateAsync(CreateRoleRequest request);

    Task<RoleDto?> UpdateAsync(int id, CreateRoleRequest request);

    Task<bool> DeleteAsync(int id);
}