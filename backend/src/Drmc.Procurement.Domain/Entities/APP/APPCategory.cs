using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Domain.Entities.APP;

public class APPCategory
{
    public int APPCategoryId { get; set; }

    public int APPId { get; set; }
    public int CategoryId { get; set; }

    public decimal TotalEstimatedBudget { get; set; }

    public APP APP { get; set; } = null!;
    public ProcurementCategory Category { get; set; } = null!;

    public ICollection<APPItem> Items { get; set; }
        = new List<APPItem>();
}