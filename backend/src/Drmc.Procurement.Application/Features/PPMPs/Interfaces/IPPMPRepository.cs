using Drmc.Procurement.Domain.Entities.Procurement;
using Drmc.Procurement.Domain.Entities.Audit;

namespace Drmc.Procurement.Application.Features.PPMPs.Interfaces;

public interface IPPMPRepository
{
    Task<PPMP?> GetForSubmissionAsync(
        int ppmpId,
        int userId,
        int departmentId,
        CancellationToken cancellationToken = default);

    Task<PPMP?> GetForApprovalAsync(
        int ppmpId,
        int departmentId,
        CancellationToken cancellationToken = default);

    Task<PPMP?> GetForReviewAsync(
        int ppmpId,
        CancellationToken cancellationToken = default);

    Task<PPMP?> GetForUpdateAsync(
        int ppmpId,
        CancellationToken cancellationToken = default);

    void AddApproval(PPMPApproval approval);

    Task SaveUpdateAsync(
        PPMP ppmp,
        IReadOnlyCollection<PPMPSectionAllocation> removedAllocations,
        IReadOnlyCollection<PPMPItem> removedItems,
        PPMPChangeHistory history,
        CancellationToken cancellationToken = default);

    Task SaveNewAsync(PPMP ppmp, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
