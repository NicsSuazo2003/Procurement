using Drmc.Procurement.Application.Features.Departments.DTO;
using Drmc.Procurement.Application.Features.Departments.Interfaces;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.Departments;

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;

    public DepartmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.DepartmentName)
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                IsActive = d.IsActive
            })
            .ToListAsync();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.DepartmentId == id)
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                IsActive = d.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentRequest request)
    {
        var departmentCode = request.DepartmentCode.Trim();
        var departmentName = request.DepartmentName.Trim();

        var codeExists = await _context.Departments
            .AnyAsync(d => d.DepartmentCode == departmentCode);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "A department with this code already exists.");
        }

        var department = new Department
        {
            DepartmentCode = departmentCode,
            DepartmentName = departmentName,
            IsActive = true
        };

        _context.Departments.Add(department);

        await _context.SaveChangesAsync();

        return new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentCode = department.DepartmentCode,
            DepartmentName = department.DepartmentName,
            IsActive = department.IsActive
        };
    }

    public async Task<DepartmentDto?> UpdateAsync(
        int id,
        CreateDepartmentRequest request)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == id);

        if (department is null)
        {
            return null;
        }

        var departmentCode = request.DepartmentCode.Trim();
        var departmentName = request.DepartmentName.Trim();

        var codeExists = await _context.Departments
            .AnyAsync(d =>
                d.DepartmentCode == departmentCode &&
                d.DepartmentId != id);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "Another department already uses this code.");
        }

        department.DepartmentCode = departmentCode;
        department.DepartmentName = departmentName;

        await _context.SaveChangesAsync();

        return new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentCode = department.DepartmentCode,
            DepartmentName = department.DepartmentName,
            IsActive = department.IsActive
        };
    }

    public async Task<bool> SetActiveStatusAsync(
        int id,
        bool isActive)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == id);

        if (department is null)
        {
            return false;
        }

        department.IsActive = isActive;

        await _context.SaveChangesAsync();

        return true;
    }
}