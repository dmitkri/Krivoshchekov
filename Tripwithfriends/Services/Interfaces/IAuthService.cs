using Tripwithfriends.DTO;

namespace Tripwithfriends.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserDto> RegisterAsync(CreateUserDto createUserDto);
    string GenerateJwtToken(UserDto user);
}


