using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tripwithfriends.DTO;
using Tripwithfriends.Services.Interfaces;

namespace Tripwithfriends.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var user = await _authService.RegisterAsync(createUserDto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации");
            return BadRequest(new { error = "Не удалось зарегистрировать пользователя" });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var response = await _authService.LoginAsync(loginDto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Неудачная попытка входа для email: {Email}", loginDto.Email);
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка конфигурации при входе: {Message}", ex.Message);
            return StatusCode(500, new { error = "Ошибка конфигурации сервера. Обратитесь к администратору." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при входе для email: {Email}. Ошибка: {Error}", 
                loginDto.Email, ex.Message);
            return StatusCode(500, new { error = $"Ошибка при входе в систему: {ex.Message}" });
        }
    }

}
