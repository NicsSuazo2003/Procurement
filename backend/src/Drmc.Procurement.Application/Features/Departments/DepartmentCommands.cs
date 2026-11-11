using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Departments.DTO;
using Drmc.Procurement.Application.Features.Departments.Interfaces;

namespace Drmc.Procurement.Application.Features.Departments.Commands;

public sealed record GetAllDepartmentsQuery : IQuery<List<DepartmentDto>>;
public sealed record GetDepartmentByIdQuery(int Id) : IQuery<DepartmentDto?>;
public sealed record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand<DepartmentDto>;
public sealed record UpdateDepartmentCommand(int Id, CreateDepartmentRequest Request) : ICommand<DepartmentDto?>;
public sealed record SetDepartmentActiveStatusCommand(int Id, bool IsActive) : ICommand<bool>;

public sealed class GetAllDepartmentsHandler(IDepartmentService service) : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentDto>>
{
    public Task<List<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetDepartmentByIdHandler(IDepartmentService service) : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    public Task<DepartmentDto?> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}

public sealed class CreateDepartmentHandler(IDepartmentService service) : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    public Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken = default) =>
        service.CreateAsync(request.Request);
}

public sealed class UpdateDepartmentHandler(IDepartmentService service) : IRequestHandler<UpdateDepartmentCommand, DepartmentDto?>
{
    public Task<DepartmentDto?> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken = default) =>
        service.UpdateAsync(request.Id, request.Request);
}

public sealed class SetDepartmentActiveStatusHandler(IDepartmentService service) : IRequestHandler<SetDepartmentActiveStatusCommand, bool>
{
    public Task<bool> Handle(SetDepartmentActiveStatusCommand request, CancellationToken cancellationToken = default) =>
        service.SetActiveStatusAsync(request.Id, request.IsActive);
}
