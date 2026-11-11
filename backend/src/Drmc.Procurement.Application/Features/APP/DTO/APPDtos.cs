namespace Drmc.Procurement.Application.Features.APP.DTO;

public class APPListDto
{
    public int APPId { get; set; }
    public int PlanningYearId { get; set; }
    public string APPReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalEstimatedBudget { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CategoryCount { get; set; }
    public int ItemCount { get; set; }
}

public class APPDetailDto
{
    public int APPId { get; set; }
    public int PlanningYearId { get; set; }
    public int CreatedBy { get; set; }
    public string APPReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalEstimatedBudget { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public List<APPCategoryDto> Categories { get; set; } = [];
}

public class APPCategoryDto
{
    public int APPCategoryId { get; set; }
    public int CategoryId { get; set; }
    public decimal TotalEstimatedBudget { get; set; }
    public List<APPItemDto> Items { get; set; } = [];
}

public class APPItemDto
{
    public int APPItemId { get; set; }
    public int? ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string? ItemType { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Specifications { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedBudget { get; set; }
    public List<APPItemSourceDto> Sources { get; set; } = [];
}

public class APPItemSourceDto
{
    public int APPItemSourceId { get; set; }
    public int PPMPItemId { get; set; }
    public int PPMPId { get; set; }
    public string PPMPReference { get; set; } = string.Empty;
    public decimal ContributedQuantity { get; set; }
    public decimal ContributedBudget { get; set; }
}

public class GenerateAPPResultDto
{
    public int APPId { get; set; }
    public string APPReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int IncludedPPMPCount { get; set; }
    public int CategoryCount { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalEstimatedBudget { get; set; }
}