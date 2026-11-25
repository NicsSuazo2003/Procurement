using Drmc.Procurement.Application.Features.PPMPs.DTO;

namespace Drmc.Procurement.Application.Features.PPMPs.Interfaces;

public interface IPPMPReadRepository
{
    Task<PagedResult<PPMPDto>> GetPagedAsync(
        int? departmentId,
        int page,
        int pageSize,
        string[]? statuses,
        int? planningYear,
        CancellationToken cancellationToken = default);

    Task<PPPMSummaryDto> GetSummaryAsync(
        int? departmentId,
        CancellationToken cancellationToken = default);

    Task<PPMPDetailDto?> GetByIdAsync(
        int id,
        int? departmentId,
        CancellationToken cancellationToken = default);

    Task<List<PPMPChangeHistoryDto>> GetChangeHistoryAsync(
        int id,
        int? departmentId,
        CancellationToken cancellationToken = default);
}
