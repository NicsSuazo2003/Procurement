using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Domain.Entities.APP;

public class APPItemSource
{
    public int APPItemSourceId { get; set; }

    public int APPItemId { get; set; }
    public int PPMPItemId { get; set; }

    public decimal ContributedQuantity { get; set; }
    public decimal ContributedBudget { get; set; }

    public APPItem APPItem { get; set; } = null!;
    public PPMPItem PPMPItem { get; set; } = null!;
}