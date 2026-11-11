using Drmc.Procurement.Application.Features.Roles.DTO;
using Drmc.Procurement.Application.Features.Roles.Interfaces;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.Roles;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        return await BuildQuery().ToListAsync();
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        return await BuildQuery()
            .FirstOrDefaultAsync(x => x.RoleId == id);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request)
    {
        var roleName = request.RoleName.Trim();
        var description = request.Description.Trim();

        if (string.IsNullOrWhiteSpace(roleName) ||
            string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException(
                "Role name and description are required.");
        }

        var exists = await _context.Roles
            .AnyAsync(x => x.RoleName.ToLower() == roleName.ToLower());

        if (exists)
            throw new InvalidOperationException("Role name already exists.");

        var role = new Role
        {
            RoleName = roleName,
            Description = description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(role.RoleId))!;
    }

    public async Task<RoleDto?> UpdateAsync(
        int id,
        CreateRoleRequest request)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(x => x.RoleId == id);

        if (role is null)
            return null;

        var roleName = request.RoleName.Trim();
        var description = request.Description.Trim();

        if (string.IsNullOrWhiteSpace(roleName) ||
            string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException(
                "Role name and description are required.");
        }

        var duplicate = await _context.Roles
            .AnyAsync(x =>
                x.RoleId != id &&
                x.RoleName.ToLower() == roleName.ToLower());

        if (duplicate)
            throw new InvalidOperationException("Role name already exists.");

        role.RoleName = roleName;
        role.Description = description;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _context.Roles
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.RoleId == id);

        if (role is null)
            return false;

        if (role.UserRoles.Any())
        {
            throw new InvalidOperationException(
                "This role cannot be deleted because it is assigned to users.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return true;
    }

    private IQueryable<RoleDto> BuildQuery()
    {
        return _context.Roles
            .AsNoTracking()
            .Select(x => new RoleDto
            {
                RoleId = x.RoleId,
                RoleName = x.RoleName,
                Description = x.Description,
                AssignedUserCount = x.UserRoles.Count()
            });
    }
}