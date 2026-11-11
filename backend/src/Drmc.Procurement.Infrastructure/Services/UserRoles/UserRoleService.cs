using Drmc.Procurement.Application.Features.UserRoles.DTO;
using Drmc.Procurement.Application.Features.UserRoles.Interfaces;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.UserRoles;

public class UserRoleService : IUserRoleService
{
    private readonly ApplicationDbContext _context;

    public UserRoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserRoleDto>> GetUserRolesAsync(int userId)
    {
        var userExists = await _context.Users
            .AnyAsync(x => x.UserId == userId);

        if (!userExists)
            throw new KeyNotFoundException("User not found.");

        return await _context.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new UserRoleDto
            {
                UserRoleId = x.UserRoleId,
                UserId = x.UserId,
                EmployeeNumber = x.User.EmployeeNumber,
                FullName = x.User.FullName,
                RoleId = x.RoleId,
                RoleName = x.Role.RoleName,
                RoleDescription = x.Role.Description
            })
            .ToListAsync();
    }

    public async Task<UserRoleDto> AssignRoleAsync(
        int userId,
        AssignUserRoleRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        var role = await _context.Roles
            .FirstOrDefaultAsync(x => x.RoleId == request.RoleId);

        if (role is null)
            throw new KeyNotFoundException("Role not found.");

        var alreadyAssigned = await _context.UserRoles
            .AnyAsync(x =>
                x.UserId == userId &&
                x.RoleId == request.RoleId);

        if (alreadyAssigned)
            throw new InvalidOperationException(
                "This role is already assigned to the user.");

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = request.RoleId
        };

        _context.UserRoles.Add(userRole);

        await _context.SaveChangesAsync();

        return new UserRoleDto
        {
            UserRoleId = userRole.UserRoleId,
            UserId = user.UserId,
            EmployeeNumber = user.EmployeeNumber,
            FullName = user.FullName,
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            RoleDescription = role.Description
        };
    }

    public async Task<bool> RemoveRoleAsync(int userId, int roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.RoleId == roleId);

        if (userRole is null)
            return false;

        _context.UserRoles.Remove(userRole);

        await _context.SaveChangesAsync();

        return true;
    }
}