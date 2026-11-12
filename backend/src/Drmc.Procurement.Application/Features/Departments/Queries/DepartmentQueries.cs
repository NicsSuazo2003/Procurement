using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Departments.DTO;
using Drmc.Procurement.Application.Features.Departments.Interfaces;

namespace Drmc.Procurement.Application.Features.Departments.Queries;

public sealed record GetAllDepartmentsQuery : IQuery<List<DepartmentDto>>;
public sealed record GetDepartmentByIdQuery(int Id) : IQuery<DepartmentDto?>;

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
