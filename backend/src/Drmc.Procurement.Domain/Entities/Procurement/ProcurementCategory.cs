using Drmc.Procurement.Domain.Entities.Planning;

using Drmc.Procurement.Domain.Entities.Audit;


namespace Drmc.Procurement.Domain.Entities.Procurement;

public class ProcurementCategory
{
    public int CategoryId { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<PlanningYearCategory> PlanningYears { get; set; }
        = new List<PlanningYearCategory>();

    public ICollection<Item> Items { get; set; }
        = new List<Item>();

    public ICollection<ProcurementCategoryAuditLog> AuditLogs { get; set; }
    = new List<ProcurementCategoryAuditLog>();
}