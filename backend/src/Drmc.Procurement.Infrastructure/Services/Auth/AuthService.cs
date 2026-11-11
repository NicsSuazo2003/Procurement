using Drmc.Procurement.Application.Features.Auth.DTO;
using Drmc.Procurement.Application.Features.Auth.Interfaces;
using Drmc.Procurement.Application.Features.Users.DTO;
using Drmc.Procurement.Domain.Entities.Identity;
using Drmc.Procurement.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        ApplicationDbContext db,
        IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto?> AuthenticateAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == email);

        // Do not reveal whether the email exists.
        if (user == null || !user.IsActive)
        {
            return null;
        }

        // Existing users may not have a password yet.
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        // Upgrade the password hash when the hasher recommends it.
        if (verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash =
                _passwordHasher.HashPassword(user, request.Password);

            await _db.SaveChangesAsync();
        }

        return new UserDto
        {
            UserId = user.UserId,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.DepartmentName,
            EmployeeNumber = user.EmployeeNumber,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            RoleNames = user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role.RoleName)
                .ToList()
        };
    }
}