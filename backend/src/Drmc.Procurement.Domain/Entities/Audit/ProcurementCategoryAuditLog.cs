using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Domain.Entities.Audit;

public class ProcurementCategoryAuditLog
{
    public int CategoryAuditLogId { get; set; }

    public int CategoryId { get; set; }

    public int ChangedByUserId { get; set; }

    // Added, Updated, Activated, or Deactivated
    public string Action { get; set; } = string.Empty;

    public string? Reason { get; set; }

    // Previous values
    public string? PreviousCategoryCode { get; set; }

    public string? PreviousCategoryName { get; set; }

    public string? PreviousDescription { get; set; }

    public bool? PreviousIsActive { get; set; }

    // Updated values
    public string? UpdatedCategoryCode { get; set; }

    public string? UpdatedCategoryName { get; set; }

    public string? UpdatedDescription { get; set; }

    public bool? UpdatedIsActive { get; set; }

    public DateTime ChangedAt { get; set; }

    // Navigation properties
    public ProcurementCategory Category { get; set; } = null!;

    public User ChangedByUser { get; set; } = null!;
}