using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Roles.DTO;
using Drmc.Procurement.Application.Features.Roles.Interfaces;

namespace Drmc.Procurement.Application.Features.Roles.Queries;

public sealed record GetAllRolesQuery : IQuery<List<RoleDto>>;
public sealed record GetRoleByIdQuery(int Id) : IQuery<RoleDto?>;

public sealed class GetAllRolesHandler(IRoleService service) : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    public Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetRoleByIdHandler(IRoleService service) : IRequestHandler<GetRoleByIdQuery, RoleDto?>
{
    public Task<RoleDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}
