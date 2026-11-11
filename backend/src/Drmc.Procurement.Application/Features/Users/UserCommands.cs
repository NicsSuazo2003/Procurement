using Drmc.Procurement.Application.Common.Messaging;
using Drmc.Procurement.Application.Features.Users.DTO;
using Drmc.Procurement.Application.Features.Users.Interfaces;

namespace Drmc.Procurement.Application.Features.Users.Commands;

public sealed record CreateUserCommand(CreateUserRequest Request) : ICommand<UserDto>;
public sealed record UpdateUserCommand(int Id, CreateUserRequest Request) : ICommand<UserDto?>;
public sealed record SetUserActiveStatusCommand(int Id, bool IsActive) : ICommand<bool>;

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
