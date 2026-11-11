using Drmc.Procurement.Application.Features.APP.Interfaces;
using Drmc.Procurement.Application.Features.Auth.Interfaces;
using Drmc.Procurement.Application.Features.Departments.Interfaces;
using Drmc.Procurement.Application.Features.DepartmentSections.Interfaces;
using Drmc.Procurement.Application.Features.Items.Interfaces;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Interfaces;
using Drmc.Procurement.Application.Features.PlanningYearItems.Interfaces;
using Drmc.Procurement.Application.Features.PlanningYears.Interfaces;
using Drmc.Procurement.Application.Features.PPMPs.Interfaces;
using Drmc.Procurement.Application.Features.ProcurementCategories.Interfaces;
using Drmc.Procurement.Application.Features.Roles.Interfaces;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Interfaces;
using Drmc.Procurement.Application.Features.UserRoles.Interfaces;
using Drmc.Procurement.Application.Features.Users.Interfaces;

using Drmc.Procurement.Domain.Entities.Identity;

using Drmc.Procurement.Infrastructure.Persistence;
using Drmc.Procurement.Infrastructure.Services.APP;
using Drmc.Procurement.Infrastructure.Services.Auth;
using Drmc.Procurement.Infrastructure.Services.Departments;
using Drmc.Procurement.Infrastructure.Services.DepartmentSections;
using Drmc.Procurement.Infrastructure.Services.Items;
using Drmc.Procurement.Infrastructure.Services.PlanningYearCategories;
using Drmc.Procurement.Infrastructure.Services.PlanningYearItems;
using Drmc.Procurement.Infrastructure.Services.PlanningYears;
using Drmc.Procurement.Infrastructure.Services.PPMPs;
using Drmc.Procurement.Infrastructure.Services.ProcurementCategories;
using Drmc.Procurement.Infrastructure.Services.Roles;
using Drmc.Procurement.Infrastructure.Services.UserCategoryPreferences;
using Drmc.Procurement.Infrastructure.Services.UserRoles;
using Drmc.Procurement.Infrastructure.Services.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Drmc.Procurement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");

#if DEBUG
        var csb = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        Console.WriteLine($"[DI] Env      = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
        Console.WriteLine($"[DI] Server   = {csb.DataSource}");
        Console.WriteLine($"[DI] Database = {csb.InitialCatalog}");
        Console.WriteLine($"[DI] WinUser  = {System.Security.Principal.WindowsIdentity.GetCurrent().Name}");
#endif

        // Pooled DbContext: reuses context instances across requests
        // and skips model-building/discovery on every request.
        // poolSize 128 gives headroom for ~20 concurrent users x
        // multiple in-flight requests; contexts share the underlying
        // SQL Server connection pool (default 100 connections).
        // IMPORTANT: register the context exactly once. A second
        // AddDbContext/AddDbContextPool call breaks resolution.
        services.AddDbContextPool<ApplicationDbContext>(
            options => options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(60);
                }),
            poolSize: 128);

        // ---- Organisation ----
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDepartmentSectionService, DepartmentSectionService>();

        // ---- Planning ----
        services.AddScoped<IPlanningYearService, PlanningYearService>();
        services.AddScoped<IPlanningYearCategoryService, PlanningYearCategoryService>();
        services.AddScoped<IPlanningYearItemService, PlanningYearItemService>();

        // ---- Procurement ----
        services.AddScoped<IProcurementCategoryService, ProcurementCategoryService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IPPMPService, PPMPService>();
        services.AddScoped<IAPPService, APPService>();

        // ---- Identity ----
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IUserCategoryPreferenceService, UserCategoryPreferenceService>();

        // ---- Auth ----
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}