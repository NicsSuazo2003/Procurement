using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Items.DTO;
using Drmc.Procurement.Application.Features.Items.Interfaces;

namespace Drmc.Procurement.Application.Features.Items.Queries;

public sealed record GetAllItemsQuery : IQuery<List<ItemDto>>;
public sealed record GetItemsByCategoryQuery(int CategoryId) : IQuery<List<ItemDto>>;
public sealed record GetItemByIdQuery(int Id) : IQuery<ItemDto?>;
public sealed record GetItemsByPlanningYearAndCategoryQuery(int PlanningYear, int CategoryId) : IQuery<List<PlanningYearCatalogueItemDto>>;

public sealed class GetAllItemsHandler(IItemService service) : IRequestHandler<GetAllItemsQuery, List<ItemDto>>
{
    public Task<List<ItemDto>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetItemsByCategoryHandler(IItemService service) : IRequestHandler<GetItemsByCategoryQuery, List<ItemDto>>
{
    public Task<List<ItemDto>> Handle(GetItemsByCategoryQuery request, CancellationToken cancellationToken = default) =>
        service.GetByCategoryIdAsync(request.CategoryId);
}

public sealed class GetItemByIdHandler(IItemService service) : IRequestHandler<GetItemByIdQuery, ItemDto?>
{
    public Task<ItemDto?> Handle(GetItemByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}

public sealed class GetItemsByPlanningYearAndCategoryHandler(IItemService service) : IRequestHandler<GetItemsByPlanningYearAndCategoryQuery, List<PlanningYearCatalogueItemDto>>
{
    public Task<List<PlanningYearCatalogueItemDto>> Handle(GetItemsByPlanningYearAndCategoryQuery request, CancellationToken cancellationToken = default) =>
        service.GetByPlanningYearAndCategoryAsync(request.PlanningYear, request.CategoryId);
}
