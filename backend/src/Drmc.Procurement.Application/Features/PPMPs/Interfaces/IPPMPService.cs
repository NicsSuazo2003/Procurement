using Drmc.Procurement.Application.Features.PPMPs.DTO;

namespace Drmc.Procurement.Application.Features.PPMPs.Interfaces;

public interface IPPMPService
{
    Task<List<PPMPDto>> GetAllAsync(int? departmentId);

    // NEW: server-side pagination + status filter.
    // departmentId == null means "no department scoping" (Procurement/Admin).
    // statuses == null or empty means "all statuses".
    Task<PagedResult<PPMPDto>> GetPagedAsync(
        int? departmentId,
        int page,
        int pageSize,
        string[]? statuses,
        int? planningYear);

    // NEW: one query, group-by-status, projected in memory.
    Task<PPPMSummaryDto> GetSummaryAsync(int? departmentId);

    Task<PPMPDetailDto?> GetByIdAsync(
        int id,
        int? departmentId);

    Task<PPMPDetailDto> CreateAsync(
        CreatePPMPRequest request,
        int createdBy,
        int departmentId);

    Task<PPMPDetailDto> SubmitAsync(
        int id,
        int userId,
        int departmentId);

    Task<PPMPDetailDto> ApproveAsync(
        int id,
        ApprovePPMPRequest request,
        int approvedBy,
        int departmentId);

    Task<PPMPDetailDto> StartReviewAsync(int id);

    Task<PPMPDetailDto> UpdateAsync(
        int id,
        UpdatePPMPRequest request,
        int changedBy);

    Task<PPMPDetailDto> CompleteReviewAsync(
        int id,
        int reviewedBy);

    Task<List<PPMPChangeHistoryDto>> GetChangeHistoryAsync(
        int id,
        int? departmentId);
}