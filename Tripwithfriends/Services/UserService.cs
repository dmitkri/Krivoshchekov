using Tripwithfriends.DTO;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories.Interfaces;
using Tripwithfriends.Services.Interfaces;

namespace Tripwithfriends.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        if (await _userRepository.EmailExistsAsync(createUserDto.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = createUserDto.Name,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            Role = createUserDto.Role
        };

        await _userRepository.AddAsync(user);

        return MapToDto(user);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!string.IsNullOrEmpty(updateUserDto.Name))
        {
            user.Name = updateUserDto.Name;
        }

        if (!string.IsNullOrEmpty(updateUserDto.Email))
        {
            if (await _userRepository.EmailExistsAsync(updateUserDto.Email) && user.Email != updateUserDto.Email)
            {
                throw new InvalidOperationException("Email already exists");
            }
            user.Email = updateUserDto.Email;
        }

        if (!string.IsNullOrEmpty(updateUserDto.Role))
        {
            user.Role = updateUserDto.Role;
        }

        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        await _userRepository.DeleteAsync(user);
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}


