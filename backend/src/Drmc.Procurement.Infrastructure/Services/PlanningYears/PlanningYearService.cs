using Drmc.Procurement.Application.Features.PlanningYears.DTO;
using Drmc.Procurement.Application.Features.PlanningYears.Interfaces;
using Drmc.Procurement.Domain.Entities.Planning;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.PlanningYears;

public class PlanningYearService : IPlanningYearService
{
    private readonly ApplicationDbContext _context;

    public PlanningYearService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlanningYearDto>> GetAllAsync()
    {
        return await _context.PlanningYears
            .AsNoTracking()
            .OrderByDescending(p => p.Year)
            .Select(p => new PlanningYearDto
            {
                PlanningYearId = p.PlanningYearId,
                Year = p.Year,
                Status = p.Status
            })
            .ToListAsync();
    }

    public async Task<PlanningYearDto?> GetByIdAsync(int id)
    {
        return await _context.PlanningYears
            .AsNoTracking()
            .Where(p => p.PlanningYearId == id)
            .Select(p => new PlanningYearDto
            {
                PlanningYearId = p.PlanningYearId,
                Year = p.Year,
                Status = p.Status
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PlanningYearDto> CreateAsync(
        CreatePlanningYearRequest request)
    {
        var yearExists = await _context.PlanningYears
            .AnyAsync(p => p.Year == request.Year);

        if (yearExists)
        {
            throw new InvalidOperationException(
                "This planning year already exists.");
        }

        var planningYear = new PlanningYear
        {
            Year = request.Year
        };

        _context.PlanningYears.Add(planningYear);

        await _context.SaveChangesAsync();

        return new PlanningYearDto
        {
            PlanningYearId = planningYear.PlanningYearId,
            Year = planningYear.Year,
            Status = planningYear.Status
        };
    }

    public async Task<PlanningYearDto?> UpdateAsync(
        int id,
        CreatePlanningYearRequest request)
    {
        var planningYear = await _context.PlanningYears
            .FirstOrDefaultAsync(p => p.PlanningYearId == id);

        if (planningYear is null)
        {
            return null;
        }

        var yearExists = await _context.PlanningYears
            .AnyAsync(p =>
                p.Year == request.Year &&
                p.PlanningYearId != id);

        if (yearExists)
        {
            throw new InvalidOperationException(
                "Another planning year with this year already exists.");
        }

        planningYear.Year = request.Year;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> UpdateStatusAsync(
        int id,
        string status)
    {
        var planningYear = await _context.PlanningYears
            .FirstOrDefaultAsync(p => p.PlanningYearId == id);

        if (planningYear is null)
        {
            return false;
        }

        var allowedStatuses = new[]
        {
            "Open",
            "Closed"
        };

        if (!allowedStatuses.Contains(status))
        {
            throw new ArgumentException(
                "Invalid planning year status. Allowed values: Open, Closed.");
        }

        planningYear.Status = status;

        await _context.SaveChangesAsync();

        return true;
    }
}