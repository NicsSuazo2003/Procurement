using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;

namespace Drmc.Procurement.Application.Features.PPMPs.Commands;

public sealed class StartPPMPReviewHandler(
    IPPMPRepository ppmpRepository,
    IPPMPReadRepository readRepository)
    : IRequestHandler<StartPPMPReviewCommand, PPMPDetailDto>
{
    public async Task<PPMPDetailDto> Handle(
        StartPPMPReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var ppmp = await ppmpRepository.GetForReviewAsync(command.Id, cancellationToken);
        if (ppmp == null)
        {
            throw new InvalidOperationException(
                $"PPMP with ID {command.Id} was not found.");
        }

        if (ppmp.Status != "Approved by Unit Head")
        {
            throw new InvalidOperationException(
                "Only PPMPs approved by the Unit Head can enter Procurement review.");
        }

        ppmp.Status = "Under Review";
        ppmp.UpdatedAt = DateTime.UtcNow;

        await ppmpRepository.SaveChangesAsync(cancellationToken);

        var result = await readRepository.GetByIdAsync(command.Id, null, cancellationToken);
        if (result == null)
        {
            throw new InvalidOperationException(
                "PPMP review was started, but its details could not be retrieved.");
        }

        return result;
    }
}
