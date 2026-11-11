using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Items.DTO;
using Drmc.Procurement.Application.Features.Items.Interfaces;

namespace Drmc.Procurement.Application.Features.Items.Commands;

public sealed record GetAllItemsQuery : IQuery<List<ItemDto>>;
public sealed record GetItemsByCategoryQuery(int CategoryId) : IQuery<List<ItemDto>>;
public sealed record GetItemByIdQuery(int Id) : IQuery<ItemDto?>;
public sealed record GetItemsByPlanningYearAndCategoryQuery(int PlanningYear, int CategoryId) : IQuery<List<PlanningYearCatalogueItemDto>>;
public sealed record CreateItemCommand(CreateItemRequest Request) : ICommand<ItemDto>;
public sealed record UpdateItemCommand(int Id, UpdateItemRequest Request) : ICommand<ItemDto?>;
public sealed record SetItemActiveStatusCommand(int Id, bool IsActive) : ICommand<bool>;

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

public sealed class CreateItemHandler(IItemService service) : IRequestHandler<CreateItemCommand, ItemDto>
{
    public Task<ItemDto> Handle(CreateItemCommand request, CancellationToken cancellationToken = default) =>
        service.CreateAsync(request.Request);
}

public sealed class UpdateItemHandler(IItemService service) : IRequestHandler<UpdateItemCommand, ItemDto?>
{
    public Task<ItemDto?> Handle(UpdateItemCommand request, CancellationToken cancellationToken = default) =>
        service.UpdateAsync(request.Id, request.Request);
}

public sealed class SetItemActiveStatusHandler(IItemService service) : IRequestHandler<SetItemActiveStatusCommand, bool>
{
    public Task<bool> Handle(SetItemActiveStatusCommand request, CancellationToken cancellationToken = default) =>
        service.SetActiveStatusAsync(request.Id, request.IsActive);
}
