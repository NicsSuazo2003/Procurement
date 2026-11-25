using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Auth.DTO;
using Drmc.Procurement.Application.Features.Auth.Interfaces;
using Drmc.Procurement.Application.Features.Users.DTO;

namespace Drmc.Procurement.Application.Features.Auth.Queries;

public sealed record AuthenticateUserQuery(LoginRequest Request) : IQuery<UserDto?>;

public sealed class AuthenticateUserHandler(IAuthService service) : IRequestHandler<AuthenticateUserQuery, UserDto?>
{
    public Task<UserDto?> Handle(AuthenticateUserQuery request, CancellationToken cancellationToken = default) =>
        service.AuthenticateAsync(request.Request);
}
