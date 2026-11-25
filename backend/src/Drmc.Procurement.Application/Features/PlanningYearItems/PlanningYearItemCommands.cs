using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PlanningYearItems.DTO;
using Drmc.Procurement.Application.Features.PlanningYearItems.Interfaces;

namespace Drmc.Procurement.Application.Features.PlanningYearItems.Commands;

public sealed record AssignPlanningYearItemCommand(AssignPlanningYearItemRequest Request) : ICommand<PlanningYearItemDto>;
public sealed record UpdatePlanningYearItemCommand(int Id, UpdatePlanningYearItemRequest Request) : ICommand<PlanningYearItemDto?>;
public sealed record SetPlanningYearItemActiveStatusCommand(int Id, bool IsActive) : ICommand<bool>;

public sealed class AssignPlanningYearItemHandler(IPlanningYearItemService service) : IRequestHandler<AssignPlanningYearItemCommand, PlanningYearItemDto>
{
    public Task<PlanningYearItemDto> Handle(AssignPlanningYearItemCommand request, CancellationToken cancellationToken = default) =>
        service.AssignAsync(request.Request);
}

public sealed class UpdatePlanningYearItemHandler(IPlanningYearItemService service) : IRequestHandler<UpdatePlanningYearItemCommand, PlanningYearItemDto?>
{
    public Task<PlanningYearItemDto?> Handle(UpdatePlanningYearItemCommand request, CancellationToken cancellationToken = default) =>
        service.UpdateAsync(request.Id, request.Request);
}

public sealed class SetPlanningYearItemActiveStatusHandler(IPlanningYearItemService service) : IRequestHandler<SetPlanningYearItemActiveStatusCommand, bool>
{
    public Task<bool> Handle(SetPlanningYearItemActiveStatusCommand request, CancellationToken cancellationToken = default) =>
        service.SetActiveStatusAsync(request.Id, request.IsActive);
}
