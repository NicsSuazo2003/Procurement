using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.PlanningYearCategories.DTO;
using Drmc.Procurement.Application.Features.PlanningYearCategories.Interfaces;

namespace Drmc.Procurement.Application.Features.PlanningYearCategories.Queries;

public sealed record GetAllPlanningYearCategoriesQuery : IQuery<List<PlanningYearCategoryDto>>;
public sealed record GetPlanningYearCategoriesByPlanningYearQuery(int PlanningYearId) : IQuery<List<PlanningYearCategoryDto>>;
public sealed record GetPlanningYearCategoryByIdQuery(int Id) : IQuery<PlanningYearCategoryDto?>;

public sealed class GetAllPlanningYearCategoriesHandler(IPlanningYearCategoryService service) : IRequestHandler<GetAllPlanningYearCategoriesQuery, List<PlanningYearCategoryDto>>
{
    public Task<List<PlanningYearCategoryDto>> Handle(GetAllPlanningYearCategoriesQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetPlanningYearCategoriesByPlanningYearHandler(IPlanningYearCategoryService service) : IRequestHandler<GetPlanningYearCategoriesByPlanningYearQuery, List<PlanningYearCategoryDto>>
{
    public Task<List<PlanningYearCategoryDto>> Handle(GetPlanningYearCategoriesByPlanningYearQuery request, CancellationToken cancellationToken = default) =>
        service.GetByPlanningYearIdAsync(request.PlanningYearId);
}

public sealed class GetPlanningYearCategoryByIdHandler(IPlanningYearCategoryService service) : IRequestHandler<GetPlanningYearCategoryByIdQuery, PlanningYearCategoryDto?>
{
    public Task<PlanningYearCategoryDto?> Handle(GetPlanningYearCategoryByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}
