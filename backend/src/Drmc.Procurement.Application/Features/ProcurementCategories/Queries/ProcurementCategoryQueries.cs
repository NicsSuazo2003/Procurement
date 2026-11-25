using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.ProcurementCategories.DTO;
using Drmc.Procurement.Application.Features.ProcurementCategories.Interfaces;

namespace Drmc.Procurement.Application.Features.ProcurementCategories.Queries;

public sealed record GetAllProcurementCategoriesQuery : IQuery<List<ProcurementCategoryDto>>;
public sealed record GetProcurementCategoryByIdQuery(int Id) : IQuery<ProcurementCategoryDto?>;
public sealed record GetProcurementCategoryHistoryQuery(int Id) : IQuery<List<ProcurementCategoryAuditLogDto>>;

public sealed class GetAllProcurementCategoriesHandler(IProcurementCategoryService service) : IRequestHandler<GetAllProcurementCategoriesQuery, List<ProcurementCategoryDto>>
{
    public Task<List<ProcurementCategoryDto>> Handle(GetAllProcurementCategoriesQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetProcurementCategoryByIdHandler(IProcurementCategoryService service) : IRequestHandler<GetProcurementCategoryByIdQuery, ProcurementCategoryDto?>
{
    public Task<ProcurementCategoryDto?> Handle(GetProcurementCategoryByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}

public sealed class GetProcurementCategoryHistoryHandler(IProcurementCategoryService service) : IRequestHandler<GetProcurementCategoryHistoryQuery, List<ProcurementCategoryAuditLogDto>>
{
    public Task<List<ProcurementCategoryAuditLogDto>> Handle(GetProcurementCategoryHistoryQuery request, CancellationToken cancellationToken = default) =>
        service.GetHistoryAsync(request.Id);
}
