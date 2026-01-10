using Tripwithfriends.DTO;

namespace Tripwithfriends.Services.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
    Task DeleteUserAsync(Guid id);
}


