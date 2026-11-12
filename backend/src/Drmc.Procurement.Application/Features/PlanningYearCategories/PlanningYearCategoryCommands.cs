using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PlanningYearCategories.DTO;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Interfaces;

namespace Drmc.Procurement.Application.Features.PlanningYearCategories.Commands;

public sealed record AssignPlanningYearCategoryCommand(AssignPlanningYearCategoryRequest Request) : ICommand<PlanningYearCategoryDto>;
public sealed record SetPlanningYearCategoryActiveStatusCommand(int Id, bool IsActive) : ICommand<bool>;

public sealed class AssignPlanningYearCategoryHandler(IPlanningYearCategoryService service) : IRequestHandler<AssignPlanningYearCategoryCommand, PlanningYearCategoryDto>
{
    public Task<PlanningYearCategoryDto> Handle(AssignPlanningYearCategoryCommand request, CancellationToken cancellationToken = default) =>
        service.AssignAsync(request.Request);
}

public sealed class SetPlanningYearCategoryActiveStatusHandler(IPlanningYearCategoryService service) : IRequestHandler<SetPlanningYearCategoryActiveStatusCommand, bool>
{
    public Task<bool> Handle(SetPlanningYearCategoryActiveStatusCommand request, CancellationToken cancellationToken = default) =>
        service.SetActiveStatusAsync(request.Id, request.IsActive);
}
