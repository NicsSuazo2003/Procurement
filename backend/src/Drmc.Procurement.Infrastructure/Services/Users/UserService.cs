using Drmc.Procurement.Application.Features.Users.DTO;
using Drmc.Procurement.Application.Features.Users.Interfaces;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.Users;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await BuildQuery().ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        return await BuildQuery()
            .FirstOrDefaultAsync(x => x.UserId == id);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        var employeeNumber = request.EmployeeNumber.Trim();
        var email = request.Email.Trim();
        var fullName = request.FullName.Trim();

        if (string.IsNullOrWhiteSpace(employeeNumber) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException(
                "Employee number, full name, and email are required.");
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(x => x.DepartmentId == request.DepartmentId.Value);

            if (!departmentExists)
                throw new KeyNotFoundException("Department not found.");
        }

        var duplicateEmployeeNumber = await _context.Users
            .AnyAsync(x => x.EmployeeNumber == employeeNumber);

        if (duplicateEmployeeNumber)
            throw new InvalidOperationException(
                "Employee number already exists.");

        var duplicateEmail = await _context.Users
            .AnyAsync(x => x.Email.ToLower() == email.ToLower());

        if (duplicateEmail)
            throw new InvalidOperationException(
                "Email address already exists.");

        var user = new User
        {
            DepartmentId = request.DepartmentId,
            EmployeeNumber = employeeNumber,
            FullName = fullName,
            Email = email,
            IsActive = true
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(user.UserId))!;
    }

    public async Task<UserDto?> UpdateAsync(
        int id,
        CreateUserRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == id);

        if (user is null)
            return null;

        var employeeNumber = request.EmployeeNumber.Trim();
        var email = request.Email.Trim();
        var fullName = request.FullName.Trim();

        if (string.IsNullOrWhiteSpace(employeeNumber) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException(
                "Employee number, full name, and email are required.");
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(x => x.DepartmentId == request.DepartmentId.Value);

            if (!departmentExists)
                throw new KeyNotFoundException("Department not found.");
        }

        var duplicateEmployeeNumber = await _context.Users
            .AnyAsync(x =>
                x.UserId != id &&
                x.EmployeeNumber == employeeNumber);

        if (duplicateEmployeeNumber)
            throw new InvalidOperationException(
                "Employee number already exists.");

        var duplicateEmail = await _context.Users
            .AnyAsync(x =>
                x.UserId != id &&
                x.Email.ToLower() == email.ToLower());

        if (duplicateEmail)
            throw new InvalidOperationException(
                "Email address already exists.");

        user.DepartmentId = request.DepartmentId;
        user.EmployeeNumber = employeeNumber;
        user.FullName = fullName;
        user.Email = email;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == id);

        if (user is null)
            return false;

        user.IsActive = isActive;

        await _context.SaveChangesAsync();

        return true;
    }

    private IQueryable<UserDto> BuildQuery()
    {
        return _context.Users
            .AsNoTracking()
            .Select(x => new UserDto
            {
                UserId = x.UserId,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null
                    ? x.Department.DepartmentName
                    : null,
                EmployeeNumber = x.EmployeeNumber,
                FullName = x.FullName,
                Email = x.Email,
                IsActive = x.IsActive,
                RoleNames = x.UserRoles
                    .Select(ur => ur.Role.RoleName)
                    .ToList()
            });
    }
}