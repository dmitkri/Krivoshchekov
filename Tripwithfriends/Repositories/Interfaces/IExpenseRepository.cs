using Tripwithfriends.DTO;
using Tripwithfriends.Models.Entities;

namespace Tripwithfriends.Repositories.Interfaces;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IEnumerable<Expense>> GetByTripIdAsync(Guid tripId);
    Task<PagedResult<Expense>> GetFilteredAsync(ExpenseFilterDto filter);
    Task<Expense?> GetByIdWithDetailsAsync(Guid id);
    Task<decimal> GetTotalPaidByUserAsync(Guid userId, Guid tripId);
    Task<decimal> GetTotalOwedByUserAsync(Guid userId, Guid tripId);
}


