
using Drmc.Procurement.Application.Features.UserCategoryPreferences.DTO;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Interfaces;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.UserCategoryPreferences;

public class UserCategoryPreferenceService : IUserCategoryPreferenceService
{
    private readonly ApplicationDbContext _context;

    public UserCategoryPreferenceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserCategoryPreferenceDto>>
        GetByUserAndPlanningYearAsync(int userId, int planningYearId)
    {
        return await _context.UserCategoryPreferences
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.PlanningYearCategory.PlanningYearId == planningYearId &&
                x.PlanningYearCategory.IsActive)
            .Select(x => new UserCategoryPreferenceDto
            {
                PlanningYearCategoryId =
                    x.PlanningYearCategoryId,

                PlanningYearId =
                    x.PlanningYearCategory.PlanningYearId,

                CategoryId =
                    x.PlanningYearCategory.CategoryId,

                CategoryCode =
                    x.PlanningYearCategory.Category.CategoryCode,

                CategoryName =
                    x.PlanningYearCategory.Category.CategoryName
            })
            .ToListAsync();
    }

    public async Task<List<UserCategoryPreferenceDto>> SaveAsync(
        int userId,
        SaveUserCategoryPreferencesRequest request)
    {
        var userExists = await _context.Users
            .AnyAsync(x => x.UserId == userId);

        if (!userExists)
            throw new KeyNotFoundException("User not found.");

        var planningYearExists = await _context.PlanningYears
            .AnyAsync(x => x.PlanningYearId == request.PlanningYearId);

        if (!planningYearExists)
            throw new KeyNotFoundException("Planning year not found.");

        var selectedIds = request.PlanningYearCategoryIds
            .Distinct()
            .ToList();

        var validCategoryIds = await _context.PlanningYearCategories
            .Where(x =>
                x.PlanningYearId == request.PlanningYearId &&
                x.IsActive &&
                selectedIds.Contains(x.PlanningYearCategoryId))
            .Select(x => x.PlanningYearCategoryId)
            .ToListAsync();

        if (validCategoryIds.Count != selectedIds.Count)
        {
            throw new InvalidOperationException(
                "One or more selected categories are invalid, inactive, or do not belong to this planning year.");
        }

        var existingPreferences = await _context.UserCategoryPreferences
            .Where(x =>
                x.UserId == userId &&
                x.PlanningYearCategory.PlanningYearId == request.PlanningYearId)
            .ToListAsync();

        _context.UserCategoryPreferences.RemoveRange(existingPreferences);

        var newPreferences = selectedIds.Select(id =>
            new UserCategoryPreference
            {
                UserId = userId,
                PlanningYearCategoryId = id
            });

        await _context.UserCategoryPreferences.AddRangeAsync(newPreferences);

        await _context.SaveChangesAsync();

        return await GetByUserAndPlanningYearAsync(
            userId,
            request.PlanningYearId);
    }
}
