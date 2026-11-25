using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.DepartmentSections.DTO;
using Drmc.Procurement.Application.Features.DepartmentSections.Interfaces;

namespace Drmc.Procurement.Application.Features.DepartmentSections.Queries;

public sealed record GetAllDepartmentSectionsQuery : IQuery<List<DepartmentSectionDto>>;
public sealed record GetDepartmentSectionsByDepartmentQuery(int DepartmentId) : IQuery<List<DepartmentSectionDto>>;
public sealed record GetDepartmentSectionByIdQuery(int Id) : IQuery<DepartmentSectionDto?>;

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
