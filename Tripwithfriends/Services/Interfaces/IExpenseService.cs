using Tripwithfriends.DTO;

namespace Tripwithfriends.Services.Interfaces;

public interface IExpenseService
{
    Task<ExpenseDto> CreateExpenseAsync(Guid tripId, CreateExpenseDto createExpenseDto);
    Task<ExpenseDto?> GetExpenseByIdAsync(Guid id);
    Task<PagedResult<ExpenseDto>> GetFilteredExpensesAsync(ExpenseFilterDto filter);
    Task<ExpenseDto> UpdateExpenseAsync(Guid id, UpdateExpenseDto updateExpenseDto);
    Task DeleteExpenseAsync(Guid id);
}


