using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tripwithfriends.Data;
using Tripwithfriends.DTO;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories.Interfaces;

namespace Tripwithfriends.Repositories;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Expense>> GetByTripIdAsync(Guid tripId)
    {
        var expenses = await _dbSet
            .Include(e => e.Payer)
            .Include(e => e.Participants)
                .ThenInclude(ep => ep.User)
            .Where(e => e.TripId == tripId)
            .ToListAsync();
        
        return expenses;
    }

    public async Task<PagedResult<Expense>> GetFilteredAsync(ExpenseFilterDto filter)
    {
        var query = _dbSet
            .Include(e => e.Payer)
            .Include(e => e.Participants)
                .ThenInclude(ep => ep.User)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(e => e.Name.Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(e => e.Category == filter.Category);
        }

        if (filter.TripId.HasValue)
        {
            query = query.Where(e => e.TripId == filter.TripId.Value);
        }

        var totalCount = await query.CountAsync();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Max(1, Math.Min(100, filter.PageSize));
        
        var items = await query
            .OrderByDescending(e => e.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Expense>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<decimal> GetTotalPaidByUserAsync(Guid userId, Guid tripId)
    {
        var totalPaid = await _dbSet
            .Where(e => e.PayerId == userId && e.TripId == tripId)
            .SumAsync(e => e.Amount);
        
        return totalPaid;
    }

    public async Task<Expense?> GetByIdWithDetailsAsync(Guid id)
    {
        var expense = await _dbSet
            .Include(e => e.Payer)
            .Include(e => e.Participants)
                .ThenInclude(ep => ep.User)
            .FirstOrDefaultAsync(e => e.Id == id);
        
        return expense;
    }

    public async Task<decimal> GetTotalOwedByUserAsync(Guid userId, Guid tripId)
    {
        var dbConnection = _context.Database.GetDbConnection();
        var connectionString = dbConnection.ConnectionString;
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string не найден");
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var sql = @"
                SELECT COALESCE(SUM(e.""Amount"" / NULLIF(participant_count.""Count"", 0)), 0) as ""TotalOwed""
                FROM ""Expenses"" e
                INNER JOIN ""ExpenseParticipants"" ep ON e.""Id"" = ep.""ExpenseId""
                INNER JOIN (
                    SELECT ""ExpenseId"", COUNT(*) as ""Count""
                    FROM ""ExpenseParticipants""
                    GROUP BY ""ExpenseId""
                ) participant_count ON e.""Id"" = participant_count.""ExpenseId""
                WHERE ep.""UserId"" = @UserId AND e.""TripId"" = @TripId
            ";

            var result = await connection.QueryFirstOrDefaultAsync<decimal?>(
                sql, 
                new { UserId = userId, TripId = tripId },
                transaction: transaction);

            await transaction.CommitAsync();
            return result ?? 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
