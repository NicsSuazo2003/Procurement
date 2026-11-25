using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Procurement;
using Drmc.Procurement.Domain.Entities.Audit;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Repositories.PPMPs;

public sealed class PPMPRepository(ApplicationDbContext context) : IPPMPRepository
{
    public Task<PPMP?> GetForSubmissionAsync(
        int ppmpId,
        int userId,
        int departmentId,
        CancellationToken cancellationToken = default) =>
        context.PPMPs
            .Include(ppmp => ppmp.Items)
            .FirstOrDefaultAsync(
                ppmp => ppmp.PPMPId == ppmpId &&
                        ppmp.DepartmentId == departmentId &&
                        ppmp.CreatedBy == userId,
                cancellationToken);

    public Task<PPMP?> GetForApprovalAsync(
        int ppmpId,
        int departmentId,
        CancellationToken cancellationToken = default) =>
        context.PPMPs
            .FirstOrDefaultAsync(
                ppmp => ppmp.PPMPId == ppmpId &&
                        ppmp.DepartmentId == departmentId,
                cancellationToken);

    public Task<PPMP?> GetForReviewAsync(
        int ppmpId,
        CancellationToken cancellationToken = default) =>
        context.PPMPs.FirstOrDefaultAsync(
            ppmp => ppmp.PPMPId == ppmpId,
            cancellationToken);

    public Task<PPMP?> GetForUpdateAsync(
        int ppmpId,
        CancellationToken cancellationToken = default) =>
        context.PPMPs
            .Include(ppmp => ppmp.Items)
                .ThenInclude(item => item.SectionAllocations)
                    .ThenInclude(allocation => allocation.Section)
            .FirstOrDefaultAsync(ppmp => ppmp.PPMPId == ppmpId, cancellationToken);

    public void AddApproval(PPMPApproval approval) =>
        context.PPMPApprovals.Add(approval);

    public async Task SaveUpdateAsync(
        PPMP ppmp,
        IReadOnlyCollection<PPMPSectionAllocation> removedAllocations,
        IReadOnlyCollection<PPMPItem> removedItems,
        PPMPChangeHistory history,
        CancellationToken cancellationToken = default)
    {
        context.PPMPSectionAllocations.RemoveRange(removedAllocations);
        foreach (var removedItem in removedItems)
        {
            context.PPMPItems.Remove(removedItem);
        }

        context.PPMPChangeHistories.Add(history);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveNewAsync(PPMP ppmp, CancellationToken cancellationToken = default)
    {
        context.PPMPs.Add(ppmp);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
