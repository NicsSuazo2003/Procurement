using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;

namespace Drmc.Procurement.Application.Features.PPMPs.Commands;

public sealed class SubmitPPMPHandler(
    IPPMPRepository ppmpRepository,
    IPPMPReadRepository readRepository)
    : IRequestHandler<SubmitPPMPCommand, PPMPDetailDto>
{
    public async Task<PPMPDetailDto> Handle(
        SubmitPPMPCommand command,
        CancellationToken cancellationToken = default)
    {
        var ppmp = await ppmpRepository.GetForSubmissionAsync(
            command.Id,
            command.UserId,
            command.DepartmentId,
            cancellationToken);

        if (ppmp == null)
        {
            throw new InvalidOperationException(
                $"PPMP with ID {command.Id} was not found.");
        }

        if (ppmp.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft PPMPs can be submitted.");
        }

        if (ppmp.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "A PPMP must contain at least one item before submission.");
        }

        ppmp.Status = "For Unit Head Approval";
        ppmp.SubmittedAt = DateTime.UtcNow;
        ppmp.UpdatedAt = DateTime.UtcNow;

        await ppmpRepository.SaveChangesAsync(cancellationToken);

        var submittedPPMP = await readRepository.GetByIdAsync(
            command.Id,
            command.DepartmentId,
            cancellationToken);
        if (submittedPPMP == null)
        {
            throw new InvalidOperationException(
                "PPMP was submitted, but its details could not be retrieved.");
        }

        return submittedPPMP;
    }
}
