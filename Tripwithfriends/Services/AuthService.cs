using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Tripwithfriends.DTO;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories.Interfaces;
using Tripwithfriends.Services.Interfaces;

namespace Tripwithfriends.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Неверный email или пароль");
        }

        var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
        if (!isPasswordCorrect)
        {
            throw new UnauthorizedAccessException("Неверный email или пароль");
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };

        var jwtToken = GenerateJwtToken(userDto);
        return new LoginResponseDto
        {
            JwtToken = jwtToken,
            User = userDto
        };
    }


    public async Task<UserDto> RegisterAsync(CreateUserDto createUserDto)
    {
        var emailExists = await _userRepository.EmailExistsAsync(createUserDto.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("Пользователь с таким email уже существует");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = createUserDto.Name,
            Email = createUserDto.Email,
            PasswordHash = passwordHash,
            Role = createUserDto.Role
        };

        await _userRepository.AddAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }

    public string GenerateJwtToken(UserDto user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT ключ не настроен");
        }
        
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "Tripwithfriends";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "Tripwithfriends";
        var expirationMinutesString = _configuration["Jwt:ExpirationMinutes"] ?? "60";
        var expirationMinutes = int.Parse(expirationMinutesString);
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        return tokenString;
    }
}
