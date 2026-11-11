using Drmc.Procurement.Application.Features.DepartmentSections.DTO;
using Drmc.Procurement.Application.Features.DepartmentSections.Interfaces;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.DepartmentSections;

public class DepartmentSectionService : IDepartmentSectionService
{
    private readonly ApplicationDbContext _context;

    public DepartmentSectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentSectionDto>> GetAllAsync()
    {
        return await _context.DepartmentSections
            .AsNoTracking()
            .OrderBy(s => s.Department.DepartmentName)
            .ThenBy(s => s.SectionName)
            .Select(s => new DepartmentSectionDto
            {
                SectionId = s.SectionId,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department.DepartmentName,
                SectionCode = s.SectionCode,
                SectionName = s.SectionName,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<DepartmentSectionDto>> GetByDepartmentIdAsync(
        int departmentId)
    {
        return await _context.DepartmentSections
            .AsNoTracking()
            .Where(s => s.DepartmentId == departmentId)
            .OrderBy(s => s.SectionName)
            .Select(s => new DepartmentSectionDto
            {
                SectionId = s.SectionId,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department.DepartmentName,
                SectionCode = s.SectionCode,
                SectionName = s.SectionName,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<DepartmentSectionDto?> GetByIdAsync(int id)
    {
        return await _context.DepartmentSections
            .AsNoTracking()
            .Where(s => s.SectionId == id)
            .Select(s => new DepartmentSectionDto
            {
                SectionId = s.SectionId,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department.DepartmentName,
                SectionCode = s.SectionCode,
                SectionName = s.SectionName,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<DepartmentSectionDto> CreateAsync(
        CreateDepartmentSectionRequest request)
    {
        var sectionCode = request.SectionCode.Trim();
        var sectionName = request.SectionName.Trim();

        var departmentExists = await _context.Departments
            .AnyAsync(d => d.DepartmentId == request.DepartmentId);

        if (!departmentExists)
        {
            throw new KeyNotFoundException(
                "The selected department does not exist.");
        }

        var codeExists = await _context.DepartmentSections
            .AnyAsync(s =>
                s.DepartmentId == request.DepartmentId &&
                s.SectionCode == sectionCode);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "A section with this code already exists in this department.");
        }

        var section = new DepartmentSection
        {
            DepartmentId = request.DepartmentId,
            SectionCode = sectionCode,
            SectionName = sectionName,
            IsActive = true
        };

        _context.DepartmentSections.Add(section);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(section.SectionId)
            ?? throw new InvalidOperationException(
                "The section was created but could not be retrieved.");
    }

    public async Task<DepartmentSectionDto?> UpdateAsync(
        int id,
        CreateDepartmentSectionRequest request)
    {
        var section = await _context.DepartmentSections
            .FirstOrDefaultAsync(s => s.SectionId == id);

        if (section is null)
        {
            return null;
        }

        var departmentExists = await _context.Departments
            .AnyAsync(d => d.DepartmentId == request.DepartmentId);

        if (!departmentExists)
        {
            throw new KeyNotFoundException(
                "The selected department does not exist.");
        }

        var sectionCode = request.SectionCode.Trim();
        var sectionName = request.SectionName.Trim();

        var codeExists = await _context.DepartmentSections
            .AnyAsync(s =>
                s.DepartmentId == request.DepartmentId &&
                s.SectionCode == sectionCode &&
                s.SectionId != id);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "Another section with this code already exists in this department.");
        }

        section.DepartmentId = request.DepartmentId;
        section.SectionCode = sectionCode;
        section.SectionName = sectionName;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> SetActiveStatusAsync(
        int id,
        bool isActive)
    {
        var section = await _context.DepartmentSections
            .FirstOrDefaultAsync(s => s.SectionId == id);

        if (section is null)
        {
            return false;
        }

        section.IsActive = isActive;

        await _context.SaveChangesAsync();

        return true;
    }
}