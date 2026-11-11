using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.APP.Commands;
using Drmc.Procurement.Application.Features.APP.DTO;
using Drmc.Procurement.Application.Features.APP.Queries;
using Drmc.Procurement.Application.Features.DepartmentSections.Commands;
using Drmc.Procurement.Application.Features.DepartmentSections.DTO;
using Drmc.Procurement.Application.Features.DepartmentSections.Queries;
using Drmc.Procurement.Application.Features.Departments.Commands;
using Drmc.Procurement.Application.Features.Departments.DTO;
using Drmc.Procurement.Application.Features.Departments.Queries;
using Drmc.Procurement.Application.Features.Items.Commands;
using Drmc.Procurement.Application.Features.Items.DTO;
using Drmc.Procurement.Application.Features.Items.Queries;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Commands;
using Drmc.Procurement.Application.Features.PlanningYearCategories.DTO;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Queries;
using Drmc.Procurement.Application.Features.PlanningYearItems.Commands;
using Drmc.Procurement.Application.Features.PlanningYearItems.DTO;
using Drmc.Procurement.Application.Features.PlanningYearItems.Queries;
using Drmc.Procurement.Application.Features.PlanningYears.Commands;
using Drmc.Procurement.Application.Features.PlanningYears.DTO;
using Drmc.Procurement.Application.Features.PlanningYears.Queries;
using Drmc.Procurement.Application.Features.ProcurementCategories.Commands;
using Drmc.Procurement.Application.Features.ProcurementCategories.DTO;
using Drmc.Procurement.Application.Features.ProcurementCategories.Queries;
using Drmc.Procurement.Application.Features.Roles.Commands;
using Drmc.Procurement.Application.Features.Roles.DTO;
using Drmc.Procurement.Application.Features.Roles.Queries;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Commands;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.DTO;
using Drmc.Procurement.Application.Features.UserCategoryPreferences.Queries;
using Drmc.Procurement.Application.Features.UserRoles.Commands;
using Drmc.Procurement.Application.Features.UserRoles.DTO;
using Drmc.Procurement.Application.Features.UserRoles.Queries;
using Drmc.Procurement.Application.Features.Users.Commands;
using Drmc.Procurement.Application.Features.Users.DTO;
using Drmc.Procurement.Application.Features.Users.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Drmc.Procurement.Infrastructure;

public static class FeatureHandlerRegistrations
{
    public static IServiceCollection AddFeatureHandlers(this IServiceCollection services)
    {
        services.AddScoped<IRequestHandler<GetAllDepartmentsQuery, List<DepartmentDto>>, GetAllDepartmentsHandler>();
        services.AddScoped<IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>, GetDepartmentByIdHandler>();
        services.AddScoped<IRequestHandler<CreateDepartmentCommand, DepartmentDto>, CreateDepartmentHandler>();
        services.AddScoped<IRequestHandler<UpdateDepartmentCommand, DepartmentDto?>, UpdateDepartmentHandler>();
        services.AddScoped<IRequestHandler<SetDepartmentActiveStatusCommand, bool>, SetDepartmentActiveStatusHandler>();

        services.AddScoped<IRequestHandler<GetAllDepartmentSectionsQuery, List<DepartmentSectionDto>>, GetAllDepartmentSectionsHandler>();
        services.AddScoped<IRequestHandler<GetDepartmentSectionsByDepartmentQuery, List<DepartmentSectionDto>>, GetDepartmentSectionsByDepartmentHandler>();
        services.AddScoped<IRequestHandler<GetDepartmentSectionByIdQuery, DepartmentSectionDto?>, GetDepartmentSectionByIdHandler>();
        services.AddScoped<IRequestHandler<CreateDepartmentSectionCommand, DepartmentSectionDto>, CreateDepartmentSectionHandler>();
        services.AddScoped<IRequestHandler<UpdateDepartmentSectionCommand, DepartmentSectionDto?>, UpdateDepartmentSectionHandler>();
        services.AddScoped<IRequestHandler<SetDepartmentSectionActiveStatusCommand, bool>, SetDepartmentSectionActiveStatusHandler>();

        services.AddScoped<IRequestHandler<GetAllPlanningYearsQuery, List<PlanningYearDto>>, GetAllPlanningYearsHandler>();
        services.AddScoped<IRequestHandler<GetPlanningYearByIdQuery, PlanningYearDto?>, GetPlanningYearByIdHandler>();
        services.AddScoped<IRequestHandler<CreatePlanningYearCommand, PlanningYearDto>, CreatePlanningYearHandler>();
        services.AddScoped<IRequestHandler<UpdatePlanningYearCommand, PlanningYearDto?>, UpdatePlanningYearHandler>();
        services.AddScoped<IRequestHandler<UpdatePlanningYearStatusCommand, bool>, UpdatePlanningYearStatusHandler>();

        services.AddScoped<IRequestHandler<GetAllPlanningYearCategoriesQuery, List<PlanningYearCategoryDto>>, GetAllPlanningYearCategoriesHandler>();
        services.AddScoped<IRequestHandler<GetPlanningYearCategoriesByPlanningYearQuery, List<PlanningYearCategoryDto>>, GetPlanningYearCategoriesByPlanningYearHandler>();
        services.AddScoped<IRequestHandler<GetPlanningYearCategoryByIdQuery, PlanningYearCategoryDto?>, GetPlanningYearCategoryByIdHandler>();
        services.AddScoped<IRequestHandler<AssignPlanningYearCategoryCommand, PlanningYearCategoryDto>, AssignPlanningYearCategoryHandler>();
        services.AddScoped<IRequestHandler<SetPlanningYearCategoryActiveStatusCommand, bool>, SetPlanningYearCategoryActiveStatusHandler>();

        services.AddScoped<IRequestHandler<GetAllPlanningYearItemsQuery, List<PlanningYearItemDto>>, GetAllPlanningYearItemsHandler>();
        services.AddScoped<IRequestHandler<GetPlanningYearItemsByPlanningYearCategoryQuery, List<PlanningYearItemDto>>, GetPlanningYearItemsByPlanningYearCategoryHandler>();
        services.AddScoped<IRequestHandler<GetPlanningYearItemByIdQuery, PlanningYearItemDto?>, GetPlanningYearItemByIdHandler>();
        services.AddScoped<IRequestHandler<AssignPlanningYearItemCommand, PlanningYearItemDto>, AssignPlanningYearItemHandler>();
        services.AddScoped<IRequestHandler<UpdatePlanningYearItemCommand, PlanningYearItemDto?>, UpdatePlanningYearItemHandler>();
        services.AddScoped<IRequestHandler<SetPlanningYearItemActiveStatusCommand, bool>, SetPlanningYearItemActiveStatusHandler>();

        services.AddScoped<IRequestHandler<GetAllProcurementCategoriesQuery, List<ProcurementCategoryDto>>, GetAllProcurementCategoriesHandler>();
        services.AddScoped<IRequestHandler<GetProcurementCategoryByIdQuery, ProcurementCategoryDto?>, GetProcurementCategoryByIdHandler>();
        services.AddScoped<IRequestHandler<GetProcurementCategoryHistoryQuery, List<ProcurementCategoryAuditLogDto>>, GetProcurementCategoryHistoryHandler>();
        services.AddScoped<IRequestHandler<CreateProcurementCategoryCommand, ProcurementCategoryDto>, CreateProcurementCategoryHandler>();
        services.AddScoped<IRequestHandler<UpdateProcurementCategoryCommand, ProcurementCategoryDto?>, UpdateProcurementCategoryHandler>();
        services.AddScoped<IRequestHandler<SetProcurementCategoryActiveStatusCommand, bool>, SetProcurementCategoryActiveStatusHandler>();

        services.AddScoped<IRequestHandler<GetAllRolesQuery, List<RoleDto>>, GetAllRolesHandler>();
        services.AddScoped<IRequestHandler<GetRoleByIdQuery, RoleDto?>, GetRoleByIdHandler>();
        services.AddScoped<IRequestHandler<CreateRoleCommand, RoleDto>, CreateRoleHandler>();
        services.AddScoped<IRequestHandler<UpdateRoleCommand, RoleDto?>, UpdateRoleHandler>();
        services.AddScoped<IRequestHandler<DeleteRoleCommand, bool>, DeleteRoleHandler>();

        services.AddScoped<IRequestHandler<GetUserCategoryPreferencesQuery, List<UserCategoryPreferenceDto>>, GetUserCategoryPreferencesHandler>();
        services.AddScoped<IRequestHandler<SaveUserCategoryPreferencesCommand, List<UserCategoryPreferenceDto>>, SaveUserCategoryPreferencesHandler>();

        services.AddScoped<IRequestHandler<GetUserRolesQuery, List<UserRoleDto>>, GetUserRolesHandler>();
        services.AddScoped<IRequestHandler<AssignUserRoleCommand, UserRoleDto>, AssignUserRoleHandler>();
        services.AddScoped<IRequestHandler<RemoveUserRoleCommand, bool>, RemoveUserRoleHandler>();

        services.AddScoped<IRequestHandler<GetAllUsersQuery, List<UserDto>>, GetAllUsersHandler>();
        services.AddScoped<IRequestHandler<GetUserByIdQuery, UserDto?>, GetUserByIdHandler>();
        services.AddScoped<IRequestHandler<CreateUserCommand, UserDto>, CreateUserHandler>();
        services.AddScoped<IRequestHandler<UpdateUserCommand, UserDto?>, UpdateUserHandler>();
        services.AddScoped<IRequestHandler<SetUserActiveStatusCommand, bool>, SetUserActiveStatusHandler>();

        services.AddScoped<IRequestHandler<GetAllItemsQuery, List<ItemDto>>, GetAllItemsHandler>();
        services.AddScoped<IRequestHandler<GetItemsByCategoryQuery, List<ItemDto>>, GetItemsByCategoryHandler>();
        services.AddScoped<IRequestHandler<GetItemByIdQuery, ItemDto?>, GetItemByIdHandler>();
        services.AddScoped<IRequestHandler<GetItemsByPlanningYearAndCategoryQuery, List<PlanningYearCatalogueItemDto>>, GetItemsByPlanningYearAndCategoryHandler>();
        services.AddScoped<IRequestHandler<CreateItemCommand, ItemDto>, CreateItemHandler>();
        services.AddScoped<IRequestHandler<UpdateItemCommand, ItemDto?>, UpdateItemHandler>();
        services.AddScoped<IRequestHandler<SetItemActiveStatusCommand, bool>, SetItemActiveStatusHandler>();

        services.AddScoped<IRequestHandler<GetAllAPPsQuery, IReadOnlyList<APPListDto>>, GetAllAPPsHandler>();
        services.AddScoped<IRequestHandler<GetAPPByIdQuery, APPDetailDto?>, GetAPPByIdHandler>();
        services.AddScoped<IRequestHandler<GenerateAPPCommand, GenerateAPPResultDto>, GenerateAPPHandler>();
        services.AddScoped<IRequestHandler<AddReviewedPPMPsCommand, GenerateAPPResultDto>, AddReviewedPPMPsHandler>();

        return services;
    }
}
