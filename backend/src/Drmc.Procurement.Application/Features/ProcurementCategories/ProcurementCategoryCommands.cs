using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.ProcurementCategories.DTO;
using Drmc.Procurement.Application.Features.ProcurementCategories.Interfaces;

namespace Drmc.Procurement.Application.Features.ProcurementCategories.Commands;

public sealed record GetAllProcurementCategoriesQuery : IQuery<List<ProcurementCategoryDto>>;
public sealed record GetProcurementCategoryByIdQuery(int Id) : IQuery<ProcurementCategoryDto?>;
public sealed record GetProcurementCategoryHistoryQuery(int Id) : IQuery<List<ProcurementCategoryAuditLogDto>>;
public sealed record CreateProcurementCategoryCommand(CreateProcurementCategoryRequest Request, int ChangedByUserId) : ICommand<ProcurementCategoryDto>;
public sealed record UpdateProcurementCategoryCommand(int Id, CreateProcurementCategoryRequest Request, int ChangedByUserId) : ICommand<ProcurementCategoryDto?>;
public sealed record SetProcurementCategoryActiveStatusCommand(int Id, bool IsActive, int ChangedByUserId) : ICommand<bool>;

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

public sealed class CreateProcurementCategoryHandler(IProcurementCategoryService service) : IRequestHandler<CreateProcurementCategoryCommand, ProcurementCategoryDto>
{
    public Task<ProcurementCategoryDto> Handle(CreateProcurementCategoryCommand request, CancellationToken cancellationToken = default) =>
        service.CreateAsync(request.Request, request.ChangedByUserId);
}

public sealed class UpdateProcurementCategoryHandler(IProcurementCategoryService service) : IRequestHandler<UpdateProcurementCategoryCommand, ProcurementCategoryDto?>
{
    public Task<ProcurementCategoryDto?> Handle(UpdateProcurementCategoryCommand request, CancellationToken cancellationToken = default) =>
        service.UpdateAsync(request.Id, request.Request, request.ChangedByUserId);
}

public sealed class SetProcurementCategoryActiveStatusHandler(IProcurementCategoryService service) : IRequestHandler<SetProcurementCategoryActiveStatusCommand, bool>
{
    public Task<bool> Handle(SetProcurementCategoryActiveStatusCommand request, CancellationToken cancellationToken = default) =>
        service.SetActiveStatusAsync(request.Id, request.IsActive, request.ChangedByUserId);
}
