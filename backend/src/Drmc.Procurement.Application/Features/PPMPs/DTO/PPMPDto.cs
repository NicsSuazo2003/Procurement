namespace Drmc.Procurement.Application.Features.PPMPs.DTO;

public class PPMPDto
{
    public int PPMPId { get; set; }
    public string PPMPReference { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public int PlanningYearId { get; set; }
    public int PlanningYear { get; set; }

    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public decimal EstimatedBudget { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int ItemCount { get; set; }
}