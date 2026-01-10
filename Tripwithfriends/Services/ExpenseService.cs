using Tripwithfriends.DTO;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories.Interfaces;
using Tripwithfriends.Services.Interfaces;

namespace Tripwithfriends.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ITripRepository _tripRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        ITripRepository tripRepository,
        IUserRepository userRepository,
        ILogger<ExpenseService> logger)
    {
        _expenseRepository = expenseRepository;
        _tripRepository = tripRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ExpenseDto> CreateExpenseAsync(Guid tripId, CreateExpenseDto createExpenseDto)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);
        if (trip == null)
        {
            throw new KeyNotFoundException("Поездка не найдена");
        }

        if (trip.IsCompleted)
        {
            throw new InvalidOperationException("Нельзя добавлять расходы в завершенную поездку");
        }

        var payer = await _userRepository.GetByIdAsync(createExpenseDto.PayerId);
        if (payer == null)
        {
            throw new KeyNotFoundException("Плательщик не найден");
        }

        var expenseDate = createExpenseDto.Date.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(createExpenseDto.Date, DateTimeKind.Utc)
            : createExpenseDto.Date.ToUniversalTime();
        
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = createExpenseDto.Name,
            Amount = createExpenseDto.Amount,
            Category = createExpenseDto.Category,
            Date = expenseDate,
            PayerId = createExpenseDto.PayerId,
            TripId = tripId
        };

        foreach (var participantId in createExpenseDto.ParticipantIds)
        {
            var userExists = await _userRepository.ExistsAsync(participantId);
            if (userExists)
            {
                var participant = new ExpenseParticipant
                {
                    ExpenseId = expense.Id,
                    UserId = participantId
                };
                expense.Participants.Add(participant);
            }
        }

        if (expense.Participants.Count == 0)
        {
            var payerParticipant = new ExpenseParticipant
            {
                ExpenseId = expense.Id,
                UserId = createExpenseDto.PayerId
            };
            expense.Participants.Add(payerParticipant);
        }

        await _expenseRepository.AddAsync(expense);
        var createdExpense = await _expenseRepository.GetByIdWithDetailsAsync(expense.Id);
        if (createdExpense == null)
        {
            throw new InvalidOperationException("Не удалось получить созданный расход");
        }

        return MapToDto(createdExpense);
    }

    public async Task<ExpenseDto?> GetExpenseByIdAsync(Guid id)
    {
        var expense = await _expenseRepository.GetByIdWithDetailsAsync(id);
        if (expense == null)
        {
            return null;
        }

        return MapToDto(expense);
    }

    public async Task<PagedResult<ExpenseDto>> GetFilteredExpensesAsync(ExpenseFilterDto filter)
    {
        var result = await _expenseRepository.GetFilteredAsync(filter);
        var expenseDtos = new List<ExpenseDto>();
        foreach (var expense in result.Items)
        {
            var expenseDto = MapToDto(expense);
            expenseDtos.Add(expenseDto);
        }

        return new PagedResult<ExpenseDto>
        {
            Items = expenseDtos,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<ExpenseDto> UpdateExpenseAsync(Guid id, UpdateExpenseDto updateExpenseDto)
    {
        var expense = await _expenseRepository.GetByIdWithDetailsAsync(id);
        if (expense == null)
        {
            throw new KeyNotFoundException("Расход не найден");
        }

        var trip = await _tripRepository.GetByIdAsync(expense.TripId);
        if (trip != null && trip.IsCompleted)
        {
            throw new InvalidOperationException("Нельзя обновлять расходы в завершенной поездке");
        }

        if (!string.IsNullOrEmpty(updateExpenseDto.Name))
        {
            expense.Name = updateExpenseDto.Name;
        }

        if (updateExpenseDto.Amount.HasValue)
        {
            expense.Amount = updateExpenseDto.Amount.Value;
        }

        if (!string.IsNullOrEmpty(updateExpenseDto.Category))
        {
            expense.Category = updateExpenseDto.Category;
        }

        if (updateExpenseDto.Date.HasValue)
        {
            var expenseDate = updateExpenseDto.Date.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(updateExpenseDto.Date.Value, DateTimeKind.Utc)
                : updateExpenseDto.Date.Value.ToUniversalTime();
            expense.Date = expenseDate;
        }

        if (updateExpenseDto.PayerId.HasValue)
        {
            var payerExists = await _userRepository.ExistsAsync(updateExpenseDto.PayerId.Value);
            if (!payerExists)
            {
                throw new KeyNotFoundException("Плательщик не найден");
            }
            expense.PayerId = updateExpenseDto.PayerId.Value;
        }

        if (updateExpenseDto.ParticipantIds != null)
        {
            expense.Participants.Clear();
            foreach (var participantId in updateExpenseDto.ParticipantIds)
            {
                var userExists = await _userRepository.ExistsAsync(participantId);
                if (userExists)
                {
                    var participant = new ExpenseParticipant
                    {
                        ExpenseId = expense.Id,
                        UserId = participantId
                    };
                    expense.Participants.Add(participant);
                }
            }
        }

        await _expenseRepository.UpdateAsync(expense);
        var updatedExpense = await _expenseRepository.GetByIdWithDetailsAsync(expense.Id);
        if (updatedExpense == null)
        {
            throw new InvalidOperationException("Не удалось получить обновленный расход");
        }

        return MapToDto(updatedExpense);
    }

    public async Task DeleteExpenseAsync(Guid id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            throw new KeyNotFoundException("Расход не найден");
        }

        var trip = await _tripRepository.GetByIdAsync(expense.TripId);
        if (trip != null && trip.IsCompleted)
        {
            throw new InvalidOperationException("Нельзя удалять расходы в завершенной поездке");
        }

        await _expenseRepository.DeleteAsync(expense);
    }

    private ExpenseDto MapToDto(Expense expense)
    {
        var expenseDto = new ExpenseDto
        {
            Id = expense.Id,
            Name = expense.Name,
            Amount = expense.Amount,
            Category = expense.Category,
            Date = expense.Date,
            PayerId = expense.PayerId,
            PayerName = expense.Payer?.Name ?? "",
            TripId = expense.TripId,
            ParticipantIds = new List<Guid>()
        };

        foreach (var participant in expense.Participants)
        {
            expenseDto.ParticipantIds.Add(participant.UserId);
        }

        return expenseDto;
    }
}
