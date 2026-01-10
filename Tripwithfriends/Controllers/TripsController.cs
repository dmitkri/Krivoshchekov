using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tripwithfriends.DTO;
using Tripwithfriends.Services.Interfaces;

namespace Tripwithfriends.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;
    private readonly ILogger<TripsController> _logger;

    public TripsController(ITripService tripService, ILogger<TripsController> logger)
    {
        _tripService = tripService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TripDto>>> GetAllTrips()
    {
        var trips = await _tripService.GetAllTripsAsync();
        return Ok(trips);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TripDto>> GetTrip(Guid id)
    {
        var trip = await _tripService.GetTripByIdAsync(id);
        
        if (trip == null)
        {
            return NotFound();
        }
        return Ok(trip);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TripDto>> CreateTrip([FromBody] CreateTripDto createTripDto)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                return BadRequest(new { error = "User ID not found in token" });
            }
            var userId = Guid.Parse(userIdString);
            var trip = await _tripService.CreateTripAsync(createTripDto, userId);
            return Ok(trip);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating trip");
            return BadRequest(new { error = "Failed to create trip" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TripDto>> UpdateTrip(Guid id, [FromBody] UpdateTripDto updateTripDto)
    {
        try
        {
           var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                return BadRequest(new { error = "User ID not found" });
            }
            var userId = Guid.Parse(userIdString);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole == null)
            {
                userRole = "User";
            }
            
            var trip = await _tripService.UpdateTripAsync(id, updateTripDto, userId, userRole);
            return Ok(trip);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTrip(Guid id)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                return BadRequest(new { error = "User ID not found" });
            }
            var userId = Guid.Parse(userIdString);
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
            
            await _tripService.DeleteTripAsync(id, userId, userRole);
            return Ok(new { message = "Поездка успешно удалена" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteTrip(Guid id)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                return BadRequest(new { error = "User ID not found" });
            }
            var userId = Guid.Parse(userIdString);
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
            
            await _tripService.CompleteTripAsync(id, userId, userRole);
            return Ok(new { message = "Поездка успешно завершена" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{id}/balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TripBalanceDto>>> GetTripBalance(Guid id)
    {
        try
        {
            var balances = await _tripService.GetTripBalanceAsync(id);
            return Ok(balances);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/debts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DebtDto>>> GetTripDebts(Guid id)
    {
        try
        {
            var debts = await _tripService.GetTripDebtsAsync(id);
            return Ok(debts);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/debts/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDebtsDto>> GetUserDebts(Guid id, Guid userId)
    {
        try
        {
            var userDebts = await _tripService.GetUserDebtsAsync(id, userId);
            if (userDebts == null)
            {
                return NotFound(new { error = "Пользователь не найден в поездке" });
            }
            return Ok(userDebts);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/my-debts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDebtsDto>> GetMyDebts(Guid id)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                return BadRequest(new { error = "User ID not found in token" });
            }
            var userId = Guid.Parse(userIdString);
            
            var userDebts = await _tripService.GetUserDebtsAsync(id, userId);
            if (userDebts == null)
            {
                return NotFound(new { error = "Вы не являетесь участником этой поездки" });
            }
            return Ok(userDebts);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
