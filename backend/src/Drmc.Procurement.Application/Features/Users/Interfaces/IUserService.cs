using Drmc.Procurement.Application.Features.Users.DTO;

namespace Drmc.Procurement.Application.Features.Users.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();

    Task<UserDto?> GetByIdAsync(int id);

    Task<UserDto> CreateAsync(CreateUserRequest request);

    Task<UserDto?> UpdateAsync(int id, CreateUserRequest request);

    Task<bool> SetActiveStatusAsync(int id, bool isActive);
}