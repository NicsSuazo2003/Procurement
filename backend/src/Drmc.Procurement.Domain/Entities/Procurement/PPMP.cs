using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Domain.Entities.Planning;

namespace Drmc.Procurement.Domain.Entities.Procurement;

public class PPMP
{
    public int PPMPId { get; set; }

    public string PPMPReference { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public int PlanningYearId { get; set; }

    public int CreatedBy { get; set; }

    public string Status { get; set; } = "Draft";

    public decimal EstimatedBudget { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Department Department { get; set; } = null!;

    public PlanningYear PlanningYear { get; set; } = null!;

    public User Creator { get; set; } = null!;

    public ICollection<PPMPItem> Items { get; set; }
        = new List<PPMPItem>();

    public ICollection<PPMPApproval> Approvals { get; set; }
        = new List<PPMPApproval>();
}