using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;

namespace Drmc.Procurement.Application.Features.PPMPs.Queries;

public sealed record GetPagedPPMPsQuery(int? DepartmentId, int Page, int PageSize, string[]? Statuses, int? PlanningYear) : IQuery<PagedResult<PPMPDto>>;
public sealed record GetPPPMSummaryQuery(int? DepartmentId) : IQuery<PPPMSummaryDto>;
public sealed record GetPPMPByIdQuery(int Id, int? DepartmentId) : IQuery<PPMPDetailDto?>;
public sealed record GetPPMPChangeHistoryQuery(int Id, int? DepartmentId) : IQuery<List<PPMPChangeHistoryDto>>;

public sealed class GetPagedPPMPsHandler(IPPMPReadRepository readRepository) : IRequestHandler<GetPagedPPMPsQuery, PagedResult<PPMPDto>>
{
    public Task<PagedResult<PPMPDto>> Handle(GetPagedPPMPsQuery query, CancellationToken cancellationToken = default) =>
        readRepository.GetPagedAsync(query.DepartmentId, query.Page, query.PageSize, query.Statuses, query.PlanningYear, cancellationToken);
}

public sealed class GetPPPMSummaryHandler(IPPMPReadRepository readRepository) : IRequestHandler<GetPPPMSummaryQuery, PPPMSummaryDto>
{
    public Task<PPPMSummaryDto> Handle(GetPPPMSummaryQuery query, CancellationToken cancellationToken = default) =>
        readRepository.GetSummaryAsync(query.DepartmentId, cancellationToken);
}

public sealed class GetPPMPByIdHandler(IPPMPReadRepository readRepository) : IRequestHandler<GetPPMPByIdQuery, PPMPDetailDto?>
{
    public Task<PPMPDetailDto?> Handle(GetPPMPByIdQuery query, CancellationToken cancellationToken = default) =>
        readRepository.GetByIdAsync(query.Id, query.DepartmentId, cancellationToken);
}

public sealed class GetPPMPChangeHistoryHandler(IPPMPReadRepository readRepository) : IRequestHandler<GetPPMPChangeHistoryQuery, List<PPMPChangeHistoryDto>>
{
    public Task<List<PPMPChangeHistoryDto>> Handle(GetPPMPChangeHistoryQuery query, CancellationToken cancellationToken = default) =>
        readRepository.GetChangeHistoryAsync(query.Id, query.DepartmentId, cancellationToken);
}
