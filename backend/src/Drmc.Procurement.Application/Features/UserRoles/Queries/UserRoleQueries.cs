using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.UserRoles.DTO;
using Drmc.Procurement.Application.Features.UserRoles.Interfaces;

namespace Drmc.Procurement.Application.Features.UserRoles.Queries;

public sealed record GetUserRolesQuery(int UserId) : IQuery<List<UserRoleDto>>;

public sealed class GetUserRolesHandler(IUserRoleService service) : IRequestHandler<GetUserRolesQuery, List<UserRoleDto>>
{
    public Task<List<UserRoleDto>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken = default) =>
        service.GetUserRolesAsync(request.UserId);
}
