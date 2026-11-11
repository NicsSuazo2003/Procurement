using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PlanningYears.DTO;
using Drmc.Procurement.Application.Features.PlanningYears.Interfaces;

namespace Drmc.Procurement.Application.Features.PlanningYears.Commands;

public sealed record GetAllPlanningYearsQuery : IQuery<List<PlanningYearDto>>;
public sealed record GetPlanningYearByIdQuery(int Id) : IQuery<PlanningYearDto?>;
public sealed record CreatePlanningYearCommand(CreatePlanningYearRequest Request) : ICommand<PlanningYearDto>;
public sealed record UpdatePlanningYearCommand(int Id, CreatePlanningYearRequest Request) : ICommand<PlanningYearDto?>;
public sealed record UpdatePlanningYearStatusCommand(int Id, string Status) : ICommand<bool>;

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

public sealed class CreatePlanningYearHandler(IPlanningYearService service) : IRequestHandler<CreatePlanningYearCommand, PlanningYearDto>
{
    public Task<PlanningYearDto> Handle(CreatePlanningYearCommand request, CancellationToken cancellationToken = default) =>
        service.CreateAsync(request.Request);
}

public sealed class UpdatePlanningYearHandler(IPlanningYearService service) : IRequestHandler<UpdatePlanningYearCommand, PlanningYearDto?>
{
    public Task<PlanningYearDto?> Handle(UpdatePlanningYearCommand request, CancellationToken cancellationToken = default) =>
        service.UpdateAsync(request.Id, request.Request);
}

public sealed class UpdatePlanningYearStatusHandler(IPlanningYearService service) : IRequestHandler<UpdatePlanningYearStatusCommand, bool>
{
    public Task<bool> Handle(UpdatePlanningYearStatusCommand request, CancellationToken cancellationToken = default) =>
        service.UpdateStatusAsync(request.Id, request.Status);
}
