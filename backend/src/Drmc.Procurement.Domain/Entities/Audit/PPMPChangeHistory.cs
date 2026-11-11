using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Domain.Entities.Audit;

public class PPMPChangeHistory
{
    public int ChangeHistoryId { get; set; }

    public int PPMPId { get; set; }
    public int ChangedBy { get; set; }

    public string Reason { get; set; } = string.Empty;

    public decimal PreviousBudget { get; set; }
    public decimal UpdatedBudget { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public PPMP PPMP { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;

    public ICollection<PPMPChangeHistoryItem> Items { get; set; }
        = new List<PPMPChangeHistoryItem>();
}