using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;

namespace Drmc.Procurement.Application.Features.PPMPs.Commands;

public sealed class CompletePPMPReviewHandler(
    IPPMPRepository ppmpRepository,
    IPPMPReferenceRepository referenceRepository,
    IPPMPReadRepository readRepository)
    : IRequestHandler<CompletePPMPReviewCommand, PPMPDetailDto>
{
    public async Task<PPMPDetailDto> Handle(
        CompletePPMPReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var ppmp = await ppmpRepository.GetForReviewAsync(command.Id, cancellationToken);
        if (ppmp == null)
        {
            throw new InvalidOperationException(
                $"PPMP with ID {command.Id} was not found.");
        }

        if (ppmp.Status != "Under Review" && ppmp.Status != "Updated")
        {
            throw new InvalidOperationException(
                "Only PPMPs under review or already updated can be marked as Reviewed.");
        }

        var reviewer = await referenceRepository.GetActiveUserAsync(
            command.ReviewedBy,
            cancellationToken);
        if (reviewer == null)
        {
            throw new InvalidOperationException(
                "The reviewer does not exist or is inactive.");
        }

        if (!await referenceRepository.IsActiveProcurementStaffAsync(
                command.ReviewedBy,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Only Procurement Staff can complete a PPMP review.");
        }

        ppmp.Status = "Reviewed";
        ppmp.UpdatedAt = DateTime.UtcNow;

        await ppmpRepository.SaveChangesAsync(cancellationToken);

        var result = await readRepository.GetByIdAsync(command.Id, null, cancellationToken);
        if (result == null)
        {
            throw new InvalidOperationException(
                "The PPMP was marked as Reviewed, but its details could not be retrieved.");
        }

        return result;
    }
}
