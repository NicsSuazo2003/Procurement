using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PPMPs.DTO;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;

namespace Drmc.Procurement.Application.Features.PPMPs.Queries;

public sealed record GetPagedPPMPsQuery(int? DepartmentId, int Page, int PageSize, string[]? Statuses, int? PlanningYear) : IQuery<PagedResult<PPMPDto>>;
public sealed record GetPPPMSummaryQuery(int? DepartmentId) : IQuery<PPPMSummaryDto>;
public sealed record GetPPMPByIdQuery(int Id, int? DepartmentId) : IQuery<PPMPDetailDto?>;
public sealed record GetPPMPChangeHistoryQuery(int Id, int? DepartmentId) : IQuery<List<PPMPChangeHistoryDto>>;

public sealed class GetPagedPPMPsHandler(IPPMPService service) : IRequestHandler<GetPagedPPMPsQuery, PagedResult<PPMPDto>>
{
    public Task<PagedResult<PPMPDto>> Handle(GetPagedPPMPsQuery query, CancellationToken cancellationToken = default) =>
        service.GetPagedAsync(query.DepartmentId, query.Page, query.PageSize, query.Statuses, query.PlanningYear);
}

public sealed class GetPPPMSummaryHandler(IPPMPService service) : IRequestHandler<GetPPPMSummaryQuery, PPPMSummaryDto>
{
    public Task<PPPMSummaryDto> Handle(GetPPPMSummaryQuery query, CancellationToken cancellationToken = default) =>
        service.GetSummaryAsync(query.DepartmentId);
}

public sealed class GetPPMPByIdHandler(IPPMPService service) : IRequestHandler<GetPPMPByIdQuery, PPMPDetailDto?>
{
    public Task<PPMPDetailDto?> Handle(GetPPMPByIdQuery query, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(query.Id, query.DepartmentId);
}

public sealed class GetPPMPChangeHistoryHandler(IPPMPService service) : IRequestHandler<GetPPMPChangeHistoryQuery, List<PPMPChangeHistoryDto>>
{
    public Task<List<PPMPChangeHistoryDto>> Handle(GetPPMPChangeHistoryQuery query, CancellationToken cancellationToken = default) =>
        service.GetChangeHistoryAsync(query.Id, query.DepartmentId);
}
