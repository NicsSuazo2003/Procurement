using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PlanningYearItems.DTO;
using Drmc.Procurement.Application.Features.PlanningYearItems.Interfaces;

namespace Drmc.Procurement.Application.Features.PlanningYearItems.Queries;

public sealed record GetAllPlanningYearItemsQuery : IQuery<List<PlanningYearItemDto>>;
public sealed record GetPlanningYearItemsByPlanningYearCategoryQuery(int PlanningYearCategoryId) : IQuery<List<PlanningYearItemDto>>;
public sealed record GetPlanningYearItemByIdQuery(int Id) : IQuery<PlanningYearItemDto?>;

public sealed class GetAllPlanningYearItemsHandler(IPlanningYearItemService service) : IRequestHandler<GetAllPlanningYearItemsQuery, List<PlanningYearItemDto>>
{
    public Task<List<PlanningYearItemDto>> Handle(GetAllPlanningYearItemsQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetPlanningYearItemsByPlanningYearCategoryHandler(IPlanningYearItemService service) : IRequestHandler<GetPlanningYearItemsByPlanningYearCategoryQuery, List<PlanningYearItemDto>>
{
    public Task<List<PlanningYearItemDto>> Handle(GetPlanningYearItemsByPlanningYearCategoryQuery request, CancellationToken cancellationToken = default) =>
        service.GetByPlanningYearCategoryIdAsync(request.PlanningYearCategoryId);
}

public sealed class GetPlanningYearItemByIdHandler(IPlanningYearItemService service) : IRequestHandler<GetPlanningYearItemByIdQuery, PlanningYearItemDto?>
{
    public Task<PlanningYearItemDto?> Handle(GetPlanningYearItemByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}
