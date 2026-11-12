using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.DepartmentSections.DTO;
using Drmc.Procurement.Application.Features.DepartmentSections.Interfaces;

namespace Drmc.Procurement.Application.Features.DepartmentSections.Commands;

public sealed record CreateDepartmentSectionCommand(CreateDepartmentSectionRequest Request) : ICommand<DepartmentSectionDto>;
public sealed record UpdateDepartmentSectionCommand(int Id, CreateDepartmentSectionRequest Request) : ICommand<DepartmentSectionDto?>;
public sealed record SetDepartmentSectionActiveStatusCommand(int Id, bool IsActive) : ICommand<bool>;

public sealed class CreateDepartmentSectionHandler(IDepartmentSectionService service) : IRequestHandler<CreateDepartmentSectionCommand, DepartmentSectionDto>
{
    public Task<DepartmentSectionDto> Handle(CreateDepartmentSectionCommand request, CancellationToken cancellationToken = default) =>
        service.CreateAsync(request.Request);
}

public sealed class UpdateDepartmentSectionHandler(IDepartmentSectionService service) : IRequestHandler<UpdateDepartmentSectionCommand, DepartmentSectionDto?>
{
    public Task<DepartmentSectionDto?> Handle(UpdateDepartmentSectionCommand request, CancellationToken cancellationToken = default) =>
        service.UpdateAsync(request.Id, request.Request);
}

public sealed class SetDepartmentSectionActiveStatusHandler(IDepartmentSectionService service) : IRequestHandler<SetDepartmentSectionActiveStatusCommand, bool>
{
    public Task<bool> Handle(SetDepartmentSectionActiveStatusCommand request, CancellationToken cancellationToken = default) =>
        service.SetActiveStatusAsync(request.Id, request.IsActive);
}
