using System.Globalization;
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

        foreach (var item in ppmp.Items)
        {
            if (string.IsNullOrWhiteSpace(item.RecommendedModeOfProcurement))
            {
                throw new InvalidOperationException(
                    $"Item '{item.ItemDescription}' must have a recommended mode of procurement.");
            }

            if (!DateTime.TryParseExact(
                    item.StartOfProcurementActivity,
                    "yyyy-MM",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var startMonth))
            {
                throw new InvalidOperationException(
                    $"Item '{item.ItemDescription}' must have a valid start month for procurement activity.");
            }

            if (!DateTime.TryParseExact(
                    item.EndOfProcurementActivity,
                    "yyyy-MM",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var endMonth))
            {
                throw new InvalidOperationException(
                    $"Item '{item.ItemDescription}' must have a valid end month for procurement activity.");
            }

            if (endMonth < startMonth)
            {
                throw new InvalidOperationException(
                    $"Item '{item.ItemDescription}' cannot end procurement activity before it starts.");
            }

            if (string.IsNullOrWhiteSpace(item.ExpectedDeliveryImplementationPeriod))
            {
                throw new InvalidOperationException(
                    $"Item '{item.ItemDescription}' must have an expected delivery or implementation period.");
            }

            if (string.IsNullOrWhiteSpace(item.SourceOfFunds))
            {
                throw new InvalidOperationException(
                    $"Item '{item.ItemDescription}' must have a source of funds.");
            }
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
