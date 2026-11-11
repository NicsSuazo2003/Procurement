using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Roles.DTO;
using Drmc.Procurement.Application.Features.Roles.Interfaces;

namespace Drmc.Procurement.Application.Features.Roles.Commands;

public sealed record CreateRoleCommand(CreateRoleRequest Request) : ICommand<RoleDto>;
public sealed record UpdateRoleCommand(int Id, CreateRoleRequest Request) : ICommand<RoleDto?>;
public sealed record DeleteRoleCommand(int Id) : ICommand<bool>;

public sealed class CreateRoleHandler(IRoleService service) : IRequestHandler<CreateRoleCommand, RoleDto>
{
    public Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken = default) =>
        service.CreateAsync(request.Request);
}

public sealed class UpdateRoleHandler(IRoleService service) : IRequestHandler<UpdateRoleCommand, RoleDto?>
{
    public Task<RoleDto?> Handle(UpdateRoleCommand request, CancellationToken cancellationToken = default) =>
        service.UpdateAsync(request.Id, request.Request);
}

public sealed class DeleteRoleHandler(IRoleService service) : IRequestHandler<DeleteRoleCommand, bool>
{
    public Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken = default) =>
        service.DeleteAsync(request.Id);
}
