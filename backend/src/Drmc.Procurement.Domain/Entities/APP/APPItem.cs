using Drmc.Procurement.Domain.Entities.Procurement;

namespace Drmc.Procurement.Domain.Entities.APP;

public class APPItem
{
    public int APPItemId { get; set; }

    public int APPCategoryId { get; set; }
    public int? ItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string? ItemType { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Specifications { get; set; }

    // Updated APP project-level snapshot fields
    public string? PAPCode { get; set; }
    public string? ObjectCode { get; set; }
    public string? ProcurementProjectName { get; set; }
    public string? GeneralDescription { get; set; }
    public string? EndUser { get; set; }
    public string? Classification { get; set; }
    public string? ModeOfProcurement { get; set; }
    public string? BidEvaluationCriteria { get; set; }
    public bool? MarginalizedSectorConsideration { get; set; }
    public bool? GreenTechnicalSpecifications { get; set; }
    public string? TargetPRSubmissionDate { get; set; }
    public string? StartOfProcurementActivity { get; set; }
    public string? EndOfProcurementActivity { get; set; }
    public string? SourceOfFunds { get; set; }
    public decimal? MOOE { get; set; }
    public decimal? CapitalOutlay { get; set; }
    public decimal? MultiYearContractCost { get; set; }
    public decimal? LifeCycleCost { get; set; }
    public string? ProcurementStrategyTools { get; set; }
    public string? ProspectiveBidders { get; set; }
    public string? PotentialObservers { get; set; }
    public string? ProcurementAgent { get; set; }
    public string? CollaboratingEntities { get; set; }
    public string? Remarks { get; set; }

    public decimal TotalQuantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedBudget { get; set; }

    public APPCategory APPCategory { get; set; } = null!;
    public Item? Item { get; set; }

    public ICollection<APPItemSource> Sources { get; set; }
        = new List<APPItemSource>();
}