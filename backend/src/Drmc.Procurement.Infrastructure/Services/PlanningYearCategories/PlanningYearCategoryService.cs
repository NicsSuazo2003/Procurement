using Drmc.Procurement.Application.Features.PlanningYearCategories.DTO;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Interfaces;
using Drmc.Procurement.Domain.Entities.Planning;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.PlanningYearCategories;

public class PlanningYearCategoryService : IPlanningYearCategoryService
{
    private readonly ApplicationDbContext _context;

    public PlanningYearCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlanningYearCategoryDto>> GetAllAsync()
    {
        return await BuildQuery().ToListAsync();
    }

    public async Task<List<PlanningYearCategoryDto>> GetByPlanningYearIdAsync(
        int planningYearId)
    {
        return await BuildQuery()
            .Where(x => x.PlanningYearId == planningYearId)
            .ToListAsync();
    }

    public async Task<PlanningYearCategoryDto?> GetByIdAsync(int id)
    {
        return await BuildQuery()
            .FirstOrDefaultAsync(x => x.PlanningYearCategoryId == id);
    }

    public async Task<PlanningYearCategoryDto> AssignAsync(
        AssignPlanningYearCategoryRequest request)
    {
        var planningYearExists = await _context.PlanningYears
            .AnyAsync(x => x.PlanningYearId == request.PlanningYearId);

        if (!planningYearExists)
            throw new KeyNotFoundException("Planning year not found.");

        var categoryExists = await _context.ProcurementCategories
            .AnyAsync(x => x.CategoryId == request.CategoryId);

        if (!categoryExists)
            throw new KeyNotFoundException("Procurement category not found.");

        var alreadyAssigned = await _context.PlanningYearCategories
            .AnyAsync(x =>
                x.PlanningYearId == request.PlanningYearId &&
                x.CategoryId == request.CategoryId);

        if (alreadyAssigned)
            throw new InvalidOperationException(
                "This category is already assigned to the planning year.");

        var assignment = new PlanningYearCategory
        {
            PlanningYearId = request.PlanningYearId,
            CategoryId = request.CategoryId,
            IsActive = true
        };

        _context.PlanningYearCategories.Add(assignment);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(assignment.PlanningYearCategoryId))!;
    }

    public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
    {
        var assignment = await _context.PlanningYearCategories
            .FirstOrDefaultAsync(x => x.PlanningYearCategoryId == id);

        if (assignment is null)
            return false;

        assignment.IsActive = isActive;

        await _context.SaveChangesAsync();

        return true;
    }

    private IQueryable<PlanningYearCategoryDto> BuildQuery()
    {
        return _context.PlanningYearCategories
            .AsNoTracking()
            .Select(x => new PlanningYearCategoryDto
            {
                PlanningYearCategoryId = x.PlanningYearCategoryId,
                PlanningYearId = x.PlanningYearId,
                Year = x.PlanningYear.Year,
                CategoryId = x.CategoryId,
                CategoryCode = x.Category.CategoryCode,
                CategoryName = x.Category.CategoryName,
                IsActive = x.IsActive
            });
    }
}
