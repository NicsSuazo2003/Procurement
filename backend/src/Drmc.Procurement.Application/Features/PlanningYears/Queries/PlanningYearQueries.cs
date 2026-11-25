using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PlanningYears.DTO;
using Drmc.Procurement.Application.Features.PlanningYears.Interfaces;

namespace Drmc.Procurement.Application.Features.PlanningYears.Queries;

public sealed record GetAllPlanningYearsQuery : IQuery<List<PlanningYearDto>>;
public sealed record GetPlanningYearByIdQuery(int Id) : IQuery<PlanningYearDto?>;

public sealed class GetAllPlanningYearsHandler(IPlanningYearService service) : IRequestHandler<GetAllPlanningYearsQuery, List<PlanningYearDto>>
{
    public Task<List<PlanningYearDto>> Handle(GetAllPlanningYearsQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetPlanningYearByIdHandler(IPlanningYearService service) : IRequestHandler<GetPlanningYearByIdQuery, PlanningYearDto?>
{
    public Task<PlanningYearDto?> Handle(GetPlanningYearByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}
