using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Items.DTO;
using Drmc.Procurement.Application.Features.Items.Interfaces;

namespace Drmc.Procurement.Application.Features.Items.Commands;

public sealed record CreateItemCommand(CreateItemRequest Request) : ICommand<ItemDto>;
public sealed record UpdateItemCommand(int Id, UpdateItemRequest Request) : ICommand<ItemDto?>;
public sealed record SetItemActiveStatusCommand(int Id, bool IsActive) : ICommand<bool>;

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
