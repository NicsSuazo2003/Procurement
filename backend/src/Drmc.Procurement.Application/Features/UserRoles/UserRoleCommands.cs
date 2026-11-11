using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.UserRoles.DTO;
using Drmc.Procurement.Application.Features.UserRoles.Interfaces;

namespace Drmc.Procurement.Application.Features.UserRoles.Commands;

public sealed record AssignUserRoleCommand(int UserId, AssignUserRoleRequest Request) : ICommand<UserRoleDto>;
public sealed record RemoveUserRoleCommand(int UserId, int RoleId) : ICommand<bool>;

public sealed class AssignUserRoleHandler(IUserRoleService service) : IRequestHandler<AssignUserRoleCommand, UserRoleDto>
{
    public Task<UserRoleDto> Handle(AssignUserRoleCommand request, CancellationToken cancellationToken = default) =>
        service.AssignRoleAsync(request.UserId, request.Request);
}

public sealed class RemoveUserRoleHandler(IUserRoleService service) : IRequestHandler<RemoveUserRoleCommand, bool>
{
    public Task<bool> Handle(RemoveUserRoleCommand request, CancellationToken cancellationToken = default) =>
        service.RemoveRoleAsync(request.UserId, request.RoleId);
}
