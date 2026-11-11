using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Auth.DTO;
using Drmc.Procurement.Application.Features.Auth.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IDispatcher dispatcher, IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await dispatcher.Send(new AuthenticateUserQuery(request), HttpContext.RequestAborted);
        if (user == null) return Unauthorized(new { message = "Invalid email or password." });

        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var encodedKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(encodedKey))
            throw new InvalidOperationException("JWT configuration is incomplete.");

        var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(encodedKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var configuredExpiry) ? configuredExpiry : 60;
        if (expiryMinutes <= 0) expiryMinutes = 60;

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(expiryMinutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email)
        };
        if (user.DepartmentId.HasValue)
            claims.Add(new Claim("department_id", user.DepartmentId.Value.ToString()));
        claims.AddRange(user.RoleNames.Distinct().Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(issuer, audience, claims, now, expiresAt, credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAt,
            User = user
        });
    }
}
