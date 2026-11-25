using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Users.DTO;
using Drmc.Procurement.Application.Features.Users.Interfaces;

namespace Drmc.Procurement.Application.Features.Users.Queries;

public sealed record GetAllUsersQuery : IQuery<List<UserDto>>;
public sealed record GetUserByIdQuery(int Id) : IQuery<UserDto?>;

public sealed class GetAllUsersHandler(IUserService service) : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    public Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken = default) =>
        service.GetAllAsync();
}

public sealed class GetUserByIdHandler(IUserService service) : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    public Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken = default) =>
        service.GetByIdAsync(request.Id);
}
