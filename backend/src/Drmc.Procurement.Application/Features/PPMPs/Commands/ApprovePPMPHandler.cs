using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Application.Features.PPMPs.Commands;

public sealed class ApprovePPMPHandler(
    IPPMPRepository ppmpRepository,
    IPPMPReferenceRepository referenceRepository,
    IPPMPReadRepository readRepository)
    : IRequestHandler<ApprovePPMPCommand, PPMPDetailDto>
{
    public async Task<PPMPDetailDto> Handle(
        ApprovePPMPCommand command,
        CancellationToken cancellationToken = default)
    {
        var ppmp = await ppmpRepository.GetForApprovalAsync(
            command.Id,
            command.DepartmentId,
            cancellationToken);

        if (ppmp == null)
        {
            throw new InvalidOperationException(
                $"PPMP with ID {command.Id} was not found.");
        }

        if (ppmp.Status != "For Unit Head Approval")
        {
            throw new InvalidOperationException(
                "Only PPMPs awaiting Unit Head approval can be approved.");
        }

        var approver = await referenceRepository.GetActiveUserAsync(
            command.ApprovedBy,
            cancellationToken);
        if (approver == null)
        {
            throw new InvalidOperationException(
                "The selected approver does not exist or is inactive.");
        }

        if (approver.DepartmentId != ppmp.DepartmentId)
        {
            throw new InvalidOperationException(
                "The approver does not belong to the PPMP department.");
        }

        ppmpRepository.AddApproval(new PPMPApproval
        {
            PPMPId = ppmp.PPMPId,
            ApprovedBy = command.ApprovedBy,
            ApprovalType = "Unit Head Approval",
            ApprovedAt = DateTime.UtcNow,
            Remarks = string.IsNullOrWhiteSpace(command.Request.Remarks)
                ? null
                : command.Request.Remarks.Trim()
        });

        ppmp.Status = "Approved by Unit Head";
        ppmp.UpdatedAt = DateTime.UtcNow;

        await ppmpRepository.SaveChangesAsync(cancellationToken);

        var approvedPPMP = await readRepository.GetByIdAsync(
            command.Id,
            command.DepartmentId,
            cancellationToken);
        if (approvedPPMP == null)
        {
            throw new InvalidOperationException(
                "PPMP was approved, but its details could not be retrieved.");
        }

        return approvedPPMP;
    }
}
