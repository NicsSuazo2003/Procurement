namespace Drmc.Procurement.Application.Features.ProcurementCategories.DTO;

public class ProcurementCategoryAuditLogDto
{
    public int CategoryAuditLogId { get; set; }

    public int CategoryId { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int ChangedByUserId { get; set; }

    public string ChangedByName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public string? PreviousCategoryCode { get; set; }

    public string? PreviousCategoryName { get; set; }

    public string? PreviousDescription { get; set; }

    public bool? PreviousIsActive { get; set; }

    public string? UpdatedCategoryCode { get; set; }

    public string? UpdatedCategoryName { get; set; }

    public string? UpdatedDescription { get; set; }

    public bool? UpdatedIsActive { get; set; }

    public DateTime ChangedAt { get; set; }
}