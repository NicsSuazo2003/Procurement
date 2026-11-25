using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Auth.DTO;
using Drmc.Procurement.Application.Features.Auth.Queries;
using Drmc.Procurement.Application.Features.Users.DTO;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Drmc.Procurement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IDispatcher dispatcher,
    IConfiguration configuration,
    ApplicationDbContext db) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await dispatcher.Send(new AuthenticateUserQuery(request), HttpContext.RequestAborted);
        if (user == null) return Unauthorized(new { message = "Invalid email or password." });

        var now = DateTime.UtcNow;
        var sessionExpiresAt = now.AddHours(GetSessionExpiryHours());
        var refreshToken = CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = HashRefreshToken(refreshToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = sessionExpiresAt,
        });
        await db.SaveChangesAsync(HttpContext.RequestAborted);

        return Ok(CreateTokenResponse(user, refreshToken, sessionExpiresAt, now));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var now = DateTime.UtcNow;
        var tokenHash = HashRefreshToken(request.RefreshToken);
        var storedToken = await db.RefreshTokens
            .Include(token => token.User)
                .ThenInclude(user => user.Department)
            .Include(token => token.User)
                .ThenInclude(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                HttpContext.RequestAborted);

        if (storedToken == null ||
            storedToken.RevokedAtUtc.HasValue ||
            storedToken.ExpiresAtUtc <= now ||
            !storedToken.User.IsActive)
        {
            return Unauthorized(new { message = "The session has expired. Please sign in again." });
        }

        var user = storedToken.User;
        var userDto = new UserDto
        {
            UserId = user.UserId,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.DepartmentName,
            EmployeeNumber = user.EmployeeNumber,
            FullName = user.FullName,
            Position = user.Position,
            Email = user.Email,
            IsActive = user.IsActive,
            RoleNames = user.UserRoles
                .Where(userRole => userRole.Role != null)
                .Select(userRole => userRole.Role.RoleName)
                .ToList(),
        };

        var newRefreshToken = CreateRefreshToken();
        storedToken.RevokedAtUtc = now;
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = HashRefreshToken(newRefreshToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = storedToken.ExpiresAtUtc,
        });

        try
        {
            await db.SaveChangesAsync(HttpContext.RequestAborted);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Unauthorized(new { message = "The session has already been refreshed. Please retry." });
        }

        return Ok(CreateTokenResponse(
            userDto,
            newRefreshToken,
            storedToken.ExpiresAtUtc,
            now));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var tokenHash = HashRefreshToken(request.RefreshToken);
        var storedToken = await db.RefreshTokens
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash && !token.RevokedAtUtc.HasValue,
                HttpContext.RequestAborted);

        if (storedToken != null)
        {
            storedToken.RevokedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(HttpContext.RequestAborted);
        }

        return NoContent();
    }

    private LoginResponse CreateTokenResponse(
        UserDto user,
        string refreshToken,
        DateTime sessionExpiresAt,
        DateTime now)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var encodedKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience) ||
            string.IsNullOrWhiteSpace(encodedKey))
        {
            throw new InvalidOperationException("JWT configuration is incomplete.");
        }

        var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(encodedKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var configuredExpiry)
            ? configuredExpiry
            : 60;
        if (expiryMinutes <= 0) expiryMinutes = 60;

        var expiresAt = Min(now.AddMinutes(expiryMinutes), sessionExpiresAt);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
        };
        if (user.DepartmentId.HasValue)
        {
            claims.Add(new Claim("department_id", user.DepartmentId.Value.ToString()));
        }

        claims.AddRange(user.RoleNames.Distinct().Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(issuer, audience, claims, now, expiresAt, credentials);
        return new LoginResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAt,
            RefreshToken = refreshToken,
            SessionExpiresAtUtc = sessionExpiresAt,
            User = user,
        };
    }

    private int GetSessionExpiryHours()
    {
        var hours = int.TryParse(configuration["Auth:SessionExpiryHours"], out var configuredHours)
            ? configuredHours
            : 8;
        return hours > 0 ? hours : 8;
    }

    private static string CreateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string HashRefreshToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static DateTime Min(DateTime left, DateTime right) => left < right ? left : right;
}
