using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Users.DTO;
using Drmc.Procurement.Application.Features.Users.Interfaces;

namespace Drmc.Procurement.Application.Features.Users.Commands;

public sealed record GetAllUsersQuery : IQuery<List<UserDto>>;
public sealed record GetUserByIdQuery(int Id) : IQuery<UserDto?>;
public sealed record CreateUserCommand(CreateUserRequest Request) : ICommand<UserDto>;
public sealed record UpdateUserCommand(int Id, CreateUserRequest Request) : ICommand<UserDto?>;
public sealed record SetUserActiveStatusCommand(int Id, bool IsActive) : ICommand<bool>;

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

public sealed class CreateUserHandler(IUserService service) : IRequestHandler<CreateUserCommand, UserDto>
{
    public Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken = default) =>
        service.CreateAsync(request.Request);
}

public sealed class UpdateUserHandler(IUserService service) : IRequestHandler<UpdateUserCommand, UserDto?>
{
    public Task<UserDto?> Handle(UpdateUserCommand request, CancellationToken cancellationToken = default) =>
        service.UpdateAsync(request.Id, request.Request);
}

public sealed class SetUserActiveStatusHandler(IUserService service) : IRequestHandler<SetUserActiveStatusCommand, bool>
{
    public Task<bool> Handle(SetUserActiveStatusCommand request, CancellationToken cancellationToken = default) =>
        service.SetActiveStatusAsync(request.Id, request.IsActive);
}
