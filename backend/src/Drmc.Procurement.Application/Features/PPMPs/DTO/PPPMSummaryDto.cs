
namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class PPPMSummaryDto
{
    public int Total { get; set; }

    // Cards used by PPMPListView and DashboardView.
    public int AwaitingAction { get; set; }
    public int CompletedReview { get; set; }

    // Per-status counts, used by the Procurement and Unit Head badges
    // and by the summary cards in the review views.
    public Dictionary<string, int> ByStatus { get; set; } = new();
}
