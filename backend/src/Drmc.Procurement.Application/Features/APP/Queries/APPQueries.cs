using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.APP.DTO;
using Drmc.Procurement.Application.Features.APP.Interfaces;

namespace Drmc.Procurement.Application.Features.APP.Queries;

public sealed record GetAllAPPsQuery : IQuery<IReadOnlyList<APPListDto>>;
public sealed record GetAPPByIdQuery(int Id) : IQuery<APPDetailDto?>;

public sealed class GetAllAPPsHandler(IAPPService service) : IRequestHandler<GetAllAPPsQuery, IReadOnlyList<APPListDto>>
{
    public Task<IReadOnlyList<APPListDto>> Handle(GetAllAPPsQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetAPPByIdHandler(IAPPService service) : IRequestHandler<GetAPPByIdQuery, APPDetailDto?>
{
    public Task<APPDetailDto?> Handle(GetAPPByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}
