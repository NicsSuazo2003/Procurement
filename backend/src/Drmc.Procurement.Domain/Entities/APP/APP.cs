using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Planning;

namespace Drmc.Procurement.Domain.Entities.APP;

public class APP
{
    public int APPId { get; set; }

    public int PlanningYearId { get; set; }
    public int CreatedBy { get; set; }

    public string APPReference { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public int RevisionNo { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? PreparedByUserId { get; set; }
    public DateTime? PreparedAt { get; set; }
    public int? RecommendedByUserId { get; set; }
    public DateTime? RecommendedAt { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public PlanningYear PlanningYear { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public User? PreparedByUser { get; set; }
    public User? RecommendedByUser { get; set; }
    public User? ApprovedByUser { get; set; }

    public ICollection<APPCategory> Categories { get; set; }
        = new List<APPCategory>();
}