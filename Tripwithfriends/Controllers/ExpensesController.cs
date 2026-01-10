using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tripwithfriends.DTO;
using Tripwithfriends.Services.Interfaces;

namespace Tripwithfriends.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(IExpenseService expenseService, ILogger<ExpensesController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ExpenseDto>>> GetAllExpenses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] Guid? tripId = null)
    {
        if (page < 1)
        {
            return BadRequest(new { error = "Страница должна быть больше 0" });
        }
        
        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "Размер страницы должен быть от 1 до 100" });
        }

        var filter = new ExpenseFilterDto
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Category = category,
            TripId = tripId
        };

        var result = await _expenseService.GetFilteredExpensesAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpenseDto>> GetExpense(Guid id)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(id);
        if (expense == null)
        {
            return NotFound();
        }
        return Ok(expense);
    }

    [HttpGet("trip/{tripId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ExpenseDto>>> GetExpensesByTrip(
        Guid tripId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null)
    {
        if (page < 1)
        {
            return BadRequest(new { error = "Страница должна быть больше 0" });
        }
        
        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "Размер страницы должен быть от 1 до 100" });
        }

        var filter = new ExpenseFilterDto
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Category = category,
            TripId = tripId
        };

        var result = await _expenseService.GetFilteredExpensesAsync(filter);
        return Ok(result);
    }

    [HttpPost("trip/{tripId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpenseDto>> CreateExpense(Guid tripId, [FromBody] CreateExpenseDto createExpenseDto)
    {
        try
        {
            var expense = await _expenseService.CreateExpenseAsync(tripId, createExpenseDto);
            return Ok(expense);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpenseDto>> UpdateExpense(Guid id, [FromBody] UpdateExpenseDto updateExpenseDto)
    {
        try
        {
            var expense = await _expenseService.UpdateExpenseAsync(id, updateExpenseDto);
            return Ok(expense);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        try
        {
            await _expenseService.DeleteExpenseAsync(id);
            return Ok(new { message = "Расход успешно удален" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
