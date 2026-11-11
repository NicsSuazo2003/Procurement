using Drmc.Procurement.Application.Features.UserRoles.DTO;

namespace Drmc.Procurement.Application.Features.UserRoles.Interfaces;

public interface IUserRoleService
{
    Task<List<UserRoleDto>> GetUserRolesAsync(int userId);

    Task<UserRoleDto> AssignRoleAsync(
        int userId,
        AssignUserRoleRequest request);

    Task<bool> RemoveRoleAsync(int userId, int roleId);
}