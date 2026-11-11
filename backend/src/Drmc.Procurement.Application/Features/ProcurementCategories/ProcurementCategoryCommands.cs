using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.ProcurementCategories.DTO;
using Drmc.Procurement.Application.Features.ProcurementCategories.Interfaces;

namespace Drmc.Procurement.Application.Features.ProcurementCategories.Commands;

public sealed record CreateProcurementCategoryCommand(CreateProcurementCategoryRequest Request, int ChangedByUserId) : ICommand<ProcurementCategoryDto>;
public sealed record UpdateProcurementCategoryCommand(int Id, CreateProcurementCategoryRequest Request, int ChangedByUserId) : ICommand<ProcurementCategoryDto?>;
public sealed record SetProcurementCategoryActiveStatusCommand(int Id, bool IsActive, int ChangedByUserId) : ICommand<bool>;

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
