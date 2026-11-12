using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Domain.Entities.Planning;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Application.Features.PPMPs.Interfaces;

public interface IPPMPReferenceRepository
{
    Task<Department?> GetActiveDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);

    Task<PlanningYear?> GetPlanningYearAsync(int planningYearId, CancellationToken cancellationToken = default);

    Task<User?> GetActiveUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> IsActiveProcurementStaffAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<int>> GetActiveCategoryIdsAsync(
        int planningYearId,
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, Item>> GetActiveItemsByIdsAsync(
        IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<int>> GetActiveDepartmentSectionIdsAsync(
        int departmentId,
        IReadOnlyCollection<int> sectionIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DepartmentSection>> GetActiveDepartmentSectionsAsync(
        int departmentId,
        IReadOnlyCollection<int> sectionIds,
        CancellationToken cancellationToken = default);
}
