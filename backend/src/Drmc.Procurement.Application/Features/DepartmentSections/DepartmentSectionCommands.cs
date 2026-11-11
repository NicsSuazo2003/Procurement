using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.DepartmentSections.DTO;
using Drmc.Procurement.Application.Features.DepartmentSections.Interfaces;

namespace Drmc.Procurement.Application.Features.DepartmentSections.Commands;

public sealed record GetAllDepartmentSectionsQuery : IQuery<List<DepartmentSectionDto>>;
public sealed record GetDepartmentSectionsByDepartmentQuery(int DepartmentId) : IQuery<List<DepartmentSectionDto>>;
public sealed record GetDepartmentSectionByIdQuery(int Id) : IQuery<DepartmentSectionDto?>;
public sealed record CreateDepartmentSectionCommand(CreateDepartmentSectionRequest Request) : ICommand<DepartmentSectionDto>;
public sealed record UpdateDepartmentSectionCommand(int Id, CreateDepartmentSectionRequest Request) : ICommand<DepartmentSectionDto?>;
public sealed record SetDepartmentSectionActiveStatusCommand(int Id, bool IsActive) : ICommand<bool>;

public sealed class GetAllDepartmentSectionsHandler(IDepartmentSectionService service) : IRequestHandler<GetAllDepartmentSectionsQuery, List<DepartmentSectionDto>>
{
    public Task<List<DepartmentSectionDto>> Handle(GetAllDepartmentSectionsQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetDepartmentSectionsByDepartmentHandler(IDepartmentSectionService service) : IRequestHandler<GetDepartmentSectionsByDepartmentQuery, List<DepartmentSectionDto>>
{
    public Task<List<DepartmentSectionDto>> Handle(GetDepartmentSectionsByDepartmentQuery request, CancellationToken cancellationToken = default) =>
        service.GetByDepartmentIdAsync(request.DepartmentId);
}

public sealed class GetDepartmentSectionByIdHandler(IDepartmentSectionService service) : IRequestHandler<GetDepartmentSectionByIdQuery, DepartmentSectionDto?>
{
    public Task<DepartmentSectionDto?> Handle(GetDepartmentSectionByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}

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
