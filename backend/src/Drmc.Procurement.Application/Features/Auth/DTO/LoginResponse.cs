using Drmc.Procurement.Application.Features.Users.DTO;

namespace Drmc.Procurement.Application.Features.Auth.DTO;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAtUtc { get; set; }

    public UserDto User { get; set; } = null!;
}