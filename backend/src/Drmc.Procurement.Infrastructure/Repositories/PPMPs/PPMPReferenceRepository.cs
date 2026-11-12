using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Domain.Entities.Planning;
using Drmc.Procurement.Domain.Entities.Procurement;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Repositories.PPMPs;

public sealed class PPMPReferenceRepository(ApplicationDbContext context) : IPPMPReferenceRepository
{
    public Task<Department?> GetActiveDepartmentAsync(
        int departmentId,
        CancellationToken cancellationToken = default) =>
        context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                department => department.DepartmentId == departmentId && department.IsActive,
                cancellationToken);

    public Task<PlanningYear?> GetPlanningYearAsync(
        int planningYearId,
        CancellationToken cancellationToken = default) =>
        context.PlanningYears
            .AsNoTracking()
            .FirstOrDefaultAsync(
                planningYear => planningYear.PlanningYearId == planningYearId,
                cancellationToken);

    public Task<User?> GetActiveUserAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.UserId == userId && user.IsActive,
                cancellationToken);

    public Task<bool> IsActiveProcurementStaffAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        context.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.UserId == userId &&
                        user.IsActive &&
                        user.UserRoles.Any(userRole => userRole.Role.RoleName == "Procurement Staff"),
                cancellationToken);

    public async Task<IReadOnlyCollection<int>> GetActiveCategoryIdsAsync(
        int planningYearId,
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken = default)
    {
        var ids = categoryIds.ToArray();
        return await context.PlanningYearCategories
            .AsNoTracking()
            .Where(assignment =>
                assignment.PlanningYearId == planningYearId &&
                assignment.IsActive &&
                assignment.Category.IsActive &&
                ids.Contains(assignment.CategoryId))
            .Select(assignment => assignment.CategoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, Item>> GetActiveItemsByIdsAsync(
        IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken = default)
    {
        var ids = itemIds.ToArray();
        return await context.Items
            .AsNoTracking()
            .Where(item => item.IsActive && ids.Contains(item.ItemId))
            .ToDictionaryAsync(item => item.ItemId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<int>> GetActiveDepartmentSectionIdsAsync(
        int departmentId,
        IReadOnlyCollection<int> sectionIds,
        CancellationToken cancellationToken = default)
    {
        var ids = sectionIds.ToArray();
        return await context.DepartmentSections
            .AsNoTracking()
            .Where(section =>
                section.DepartmentId == departmentId &&
                section.IsActive &&
                ids.Contains(section.SectionId))
            .Select(section => section.SectionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, DepartmentSection>> GetActiveDepartmentSectionsAsync(
        int departmentId,
        IReadOnlyCollection<int> sectionIds,
        CancellationToken cancellationToken = default)
    {
        var ids = sectionIds.ToArray();
        return await context.DepartmentSections
            .AsNoTracking()
            .Where(section =>
                section.DepartmentId == departmentId &&
                section.IsActive &&
                ids.Contains(section.SectionId))
            .ToDictionaryAsync(section => section.SectionId, cancellationToken);
    }
}
