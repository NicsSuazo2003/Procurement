
using System.Security.Claims;
using Microsoft.OpenApi;
using Drmc.Procurement.Infrastructure;
using Drmc.Procurement.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Drmc.Procurement.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// =========================================================
// CONTROLLERS
// =========================================================

builder.Services.AddControllers();

// =========================================================
// SWAGGER
// =========================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});
// =========================================================
// INFRASTRUCTURE AND DATABASE
// =========================================================

builder.Services.AddInfrastructure(builder.Configuration);


// =========================================================
// DEVELOPMENT-ONLY TEST USER PASSWORD PROVISIONING
// =========================================================

if (args.Contains("--provision-user",
    StringComparer.OrdinalIgnoreCase))
{
    if (!builder.Environment.IsDevelopment())
    {
        Console.WriteLine(
            "Password provisioning is only allowed in Development.");

        return;
    }

    var argumentIndex = Array.FindIndex(
        args,
        arg => string.Equals(
            arg,
            "--provision-user",
            StringComparison.OrdinalIgnoreCase));

    if (argumentIndex + 1 >= args.Length ||
        !int.TryParse(args[argumentIndex + 1], out var userId))
    {
        Console.WriteLine(
            "Usage: dotnet run -- --provision-user <userId>");

        return;
    }

    var expectedRoles = new Dictionary<int, string>
    {
        [2] = "End-user",
        [3] = "Unit Head",
        [4] = "Procurement Staff"
    };

    if (!expectedRoles.TryGetValue(userId, out var expectedRole))
    {
        Console.WriteLine(
            "Only test user IDs 2, 3, and 4 can be provisioned.");

        return;
    }

    await using var provisioningApp = builder.Build();

    await using var scope =
        provisioningApp.Services.CreateAsyncScope();

    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var passwordHasher = scope.ServiceProvider
        .GetRequiredService<IPasswordHasher<User>>();

    if (!await db.Database.CanConnectAsync())
    {
        Console.WriteLine("Cannot connect to the database.");
        return;
    }

    var user = await db.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .SingleOrDefaultAsync(u => u.UserId == userId);

    if (user == null)
    {
        Console.WriteLine("User was not found.");
        return;
    }

    if (!user.IsActive)
    {
        Console.WriteLine("User account is inactive.");
        return;
    }

    if (!string.IsNullOrWhiteSpace(user.PasswordHash))
    {
        Console.WriteLine(
            "This user already has a password. Provisioning stopped.");

        return;
    }

    var assignedRoles = user.UserRoles
        .Select(ur => ur.Role.RoleName)
        .ToList();

    if (assignedRoles.Count != 1 ||
        assignedRoles[0] != expectedRole)
    {
        Console.WriteLine(
            $"Expected role: {expectedRole}");

        Console.WriteLine(
            "User role assignment does not match. Provisioning stopped.");

        return;
    }

    Console.WriteLine();
    Console.WriteLine("TEST USER PASSWORD SETUP");
    Console.WriteLine($"User ID: {user.UserId}");
    Console.WriteLine($"Name: {user.FullName}");
    Console.WriteLine($"Email: {user.Email}");
    Console.WriteLine($"Role: {expectedRole}");
    Console.WriteLine();

    var password = ReadPassword("Enter new password: ");

    if (password.Length < 12)
    {
        Console.WriteLine(
            "Password must contain at least 12 characters.");

        return;
    }

    var confirmation = ReadPassword("Confirm new password: ");

    if (password != confirmation)
    {
        Console.WriteLine("Passwords do not match.");
        return;
    }

    user.PasswordHash =
        passwordHasher.HashPassword(user, password);

    await db.SaveChangesAsync();

    Console.WriteLine();
    Console.WriteLine(
        "Test user password successfully configured.");

    return;
}

// =========================================================
// JWT CONFIGURATION
// =========================================================

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

var keyBytes = Convert.FromBase64String(jwtKey);

if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must contain at least 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(keyBytes),

                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
    });

// =========================================================
// AUTHORIZATION
// =========================================================

builder.Services.AddAuthorization(options =>
{
    // All endpoints require authentication by default.
    // Endpoints such as login must explicitly use [AllowAnonymous].
    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

// =========================================================
// BUILD APPLICATION
// =========================================================

var app = builder.Build();

// =========================================================
// DEVELOPMENT TOOLS
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Database connection test
    app.MapGet("/api/health/database", async (
        ApplicationDbContext db) =>
    {
        try
        {
            var connected =
                await db.Database.CanConnectAsync();

            return connected
                ? Results.Ok(new
                {
                    status = "Connected",
                    database = "PROCUREMENT"
                })
                : Results.Problem(
                    title: "Database connection failed");
        }
        catch (Exception)
        {
            return Results.Problem(
                title: "Database connection failed",
                detail: "Check SQL Server, connection settings, and Windows permissions.");
        }
    })
    .WithTags("Database")
    .WithName("TestDatabaseConnection");
}

// =========================================================
// MIDDLEWARE
// =========================================================

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

static string ReadPassword(string prompt)
{
    Console.Write(prompt);

    var password = new System.Text.StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password.Length--;
                Console.Write("\b \b");
            }

            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
    }

    return password.ToString();
}
