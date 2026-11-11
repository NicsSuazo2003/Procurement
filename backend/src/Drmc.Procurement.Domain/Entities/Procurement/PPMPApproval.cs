using Drmc.Procurement.Domain.Entities.Identity;

namespace Drmc.Procurement.Domain.Entities.Procurement;

public class PPMPApproval
{
    public int ApprovalId { get; set; }

    public int PPMPId { get; set; }

    public int ApprovedBy { get; set; }

    public string ApprovalType { get; set; } = "Unit Head Approval";

    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;

    public string? Remarks { get; set; }

    // Navigation properties
    public PPMP PPMP { get; set; } = null!;

    public User Approver { get; set; } = null!;
}