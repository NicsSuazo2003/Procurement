using Drmc.Procurement.Application.Features.Auth.DTO;
using Drmc.Procurement.Application.Features.Users.DTO;

namespace Drmc.Procurement.Application.Features.Auth.Interfaces;

public interface IAuthService
{
    Task<UserDto?> AuthenticateAsync(LoginRequest request);
}