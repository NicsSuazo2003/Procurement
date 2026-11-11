using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Drmc.Procurement.Application.Features.Auth.DTO;
using Drmc.Procurement.Application.Features.Auth.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user = await _authService.AuthenticateAsync(request);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var encodedKey = _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience) ||
            string.IsNullOrWhiteSpace(encodedKey))
        {
            throw new InvalidOperationException(
                "JWT configuration is incomplete.");
        }

        var keyBytes = Convert.FromBase64String(encodedKey);

        var signingKey = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.TryParse(
            _configuration["Jwt:ExpiryMinutes"],
            out var configuredExpiry)
                ? configuredExpiry
                : 60;

        if (expiryMinutes <= 0)
        {
            expiryMinutes = 60;
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new(
                ClaimTypes.Name,
                user.FullName),

            new(
                ClaimTypes.Email,
                user.Email)
        };

        if (user.DepartmentId.HasValue)
        {
            claims.Add(new Claim(
                "department_id",
                user.DepartmentId.Value.ToString()));
        }

        foreach (var roleName in user.RoleNames.Distinct())
        {
            claims.Add(new Claim(
                ClaimTypes.Role,
                roleName));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken =
            new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAt,
            User = user
        });
    }
}