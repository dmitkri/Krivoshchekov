using Microsoft.Extensions.Caching.Distributed;
using System.Linq;
using System.Text.Json;
using Tripwithfriends.DTO;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories.Interfaces;
using Tripwithfriends.Services.Interfaces;

namespace Tripwithfriends.Services;

public class TripService : ITripService
{
    private readonly ITripRepository _tripRepository;
    private readonly IUserRepository _userRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TripService> _logger;
    private const string CacheKeyPrefix = "trip_";
    private const int CacheExpirationMinutes = 5;

    public TripService(
        ITripRepository tripRepository,
        IUserRepository userRepository,
        IExpenseRepository expenseRepository,
        IDistributedCache cache,
        ILogger<TripService> logger)
    {
        _tripRepository = tripRepository;
        _userRepository = userRepository;
        _expenseRepository = expenseRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TripDto> CreateTripAsync(CreateTripDto createTripDto, Guid organizerId)
    {
        var organizer = await _userRepository.GetByIdAsync(organizerId);
        if (organizer == null)
        {
            throw new KeyNotFoundException("Организатор не найден");
        }

        var startDate = createTripDto.StartDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(createTripDto.StartDate, DateTimeKind.Utc)
            : createTripDto.StartDate.ToUniversalTime();
        var endDate = createTripDto.EndDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(createTripDto.EndDate, DateTimeKind.Utc)
            : createTripDto.EndDate.ToUniversalTime();
        
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = createTripDto.Name,
            StartDate = startDate,
            EndDate = endDate,
            OrganizerId = organizerId,
            IsCompleted = false
        };

        var organizerParticipant = new TripParticipant
        {
            TripId = trip.Id,
            UserId = organizerId
        };
        trip.Participants.Add(organizerParticipant);
        foreach (var participantId in createTripDto.ParticipantIds)
        {
            if (participantId != organizerId)
            {
                var userExists = await _userRepository.ExistsAsync(participantId);
                if (userExists)
                {
                    var participant = new TripParticipant
                    {
                        TripId = trip.Id,
                        UserId = participantId
                    };
                    trip.Participants.Add(participant);
                }
            }
        }

        await _tripRepository.AddAsync(trip);
        var createdTrip = await _tripRepository.GetByIdWithDetailsAsync(trip.Id);
        if (createdTrip == null)
        {
            throw new InvalidOperationException("Не удалось получить созданную поездку");
        }
        return MapToDto(createdTrip);
    }

    public async Task<TripDto?> GetTripByIdAsync(Guid id)
    {
        var cacheKey = CacheKeyPrefix + id.ToString();
        var cachedTripJson = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedTripJson))
        {
            var cachedTrip = JsonSerializer.Deserialize<TripDto>(cachedTripJson);
            if (cachedTrip != null)
            {
                return cachedTrip;
            }
        }

        var trip = await _tripRepository.GetByIdWithDetailsAsync(id);
        if (trip == null)
        {
            return null;
        }

        var tripDto = MapToDto(trip);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes)
        };
        var tripJson = JsonSerializer.Serialize(tripDto);
        await _cache.SetStringAsync(cacheKey, tripJson, cacheOptions);

        return tripDto;
    }

    public async Task<IEnumerable<TripDto>> GetAllTripsAsync()
    {
        var trips = await _tripRepository.GetAllWithDetailsAsync();
        var result = new List<TripDto>();
        foreach (var trip in trips)
        {
            var tripDto = MapToDto(trip);
            result.Add(tripDto);
        }
        
        return result;
    }

    public async Task<TripDto> UpdateTripAsync(Guid id, UpdateTripDto updateTripDto, Guid userId, string userRole)
    {
        var trip = await _tripRepository.GetByIdWithDetailsAsync(id);
        if (trip == null)
        {
            throw new KeyNotFoundException("Поездка не найдена");
        }

        if (trip.OrganizerId != userId && userRole != "Admin" && userRole != "Manager")
        {
            throw new UnauthorizedAccessException("У вас нет прав для обновления этой поездки");
        }

        if (trip.IsCompleted)
        {
            throw new InvalidOperationException("Нельзя обновлять завершенную поездку");
        }

        if (!string.IsNullOrEmpty(updateTripDto.Name))
        {
            trip.Name = updateTripDto.Name;
        }

        if (updateTripDto.StartDate.HasValue)
        {
            var startDate = updateTripDto.StartDate.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(updateTripDto.StartDate.Value, DateTimeKind.Utc)
                : updateTripDto.StartDate.Value.ToUniversalTime();
            trip.StartDate = startDate;
        }

        if (updateTripDto.EndDate.HasValue)
        {
            var endDate = updateTripDto.EndDate.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(updateTripDto.EndDate.Value, DateTimeKind.Utc)
                : updateTripDto.EndDate.Value.ToUniversalTime();
            trip.EndDate = endDate;
        }

        if (updateTripDto.ParticipantIds != null)
        {
            var organizerId = trip.OrganizerId;
            trip.Participants.Clear();
            foreach (var participantId in updateTripDto.ParticipantIds)
            {
                if (participantId != organizerId)
                {
                    var userExists = await _userRepository.ExistsAsync(participantId);
                    if (userExists)
                    {
                        var participant = new TripParticipant
                        {
                            TripId = trip.Id,
                            UserId = participantId
                        };
                        trip.Participants.Add(participant);
                    }
                }
            }
            
            var organizerExists = trip.Participants.Any(p => p.UserId == organizerId);
            if (!organizerExists)
            {
                var organizerParticipant = new TripParticipant
                {
                    TripId = trip.Id,
                    UserId = organizerId
                };
                trip.Participants.Add(organizerParticipant);
            }
        }

        await _tripRepository.UpdateAsync(trip);
        await InvalidateCacheAsync(id);
        var updatedTrip = await _tripRepository.GetByIdWithDetailsAsync(id);
        if (updatedTrip == null)
        {
            throw new InvalidOperationException("Не удалось получить обновленную поездку");
        }

        return MapToDto(updatedTrip);
    }

    public async Task DeleteTripAsync(Guid id, Guid userId, string userRole)
    {
        var trip = await _tripRepository.GetByIdAsync(id);
        if (trip == null)
        {
            throw new KeyNotFoundException("Поездка не найдена");
        }

        if (trip.OrganizerId != userId && userRole != "Admin")
        {
            throw new UnauthorizedAccessException("У вас нет прав для удаления этой поездки");
        }

        await _tripRepository.DeleteAsync(trip);
        await InvalidateCacheAsync(id);
    }

    public async Task CompleteTripAsync(Guid id, Guid userId, string userRole)
    {
        var trip = await _tripRepository.GetByIdAsync(id);
        if (trip == null)
        {
            throw new KeyNotFoundException("Поездка не найдена");
        }

        if (trip.OrganizerId != userId && userRole != "Admin" && userRole != "Manager")
        {
            throw new UnauthorizedAccessException("У вас нет прав для завершения этой поездки");
        }

        trip.IsCompleted = true;
        await _tripRepository.UpdateAsync(trip);
        await InvalidateCacheAsync(id);
    }

    public async Task<List<TripBalanceDto>> GetTripBalanceAsync(Guid tripId)
    {
        var trip = await _tripRepository.GetByIdWithDetailsAsync(tripId);
        if (trip == null)
        {
            throw new KeyNotFoundException("Поездка не найдена");
        }

        var balances = new List<TripBalanceDto>();
        foreach (var participant in trip.Participants)
        {
            var paidAmount = await _expenseRepository.GetTotalPaidByUserAsync(participant.UserId, tripId);
            var owedAmount = await _expenseRepository.GetTotalOwedByUserAsync(participant.UserId, tripId);
            var balance = paidAmount - owedAmount;
            var balanceDto = new TripBalanceDto
            {
                UserId = participant.UserId,
                UserName = participant.User?.Name ?? "Неизвестно",
                PaidAmount = paidAmount,
                OwedAmount = owedAmount,
                Balance = balance
            };
            
            balances.Add(balanceDto);
        }

        return balances;
    }

    public async Task<List<DebtDto>> GetTripDebtsAsync(Guid tripId)
    {
        var balances = await GetTripBalanceAsync(tripId);
        var debts = new List<DebtDto>();
        var debtors = balances
            .Where(b => b.Balance < 0)
            .Select(b => new { b.UserId, b.UserName, Debt = Math.Abs(b.Balance) })
            .OrderByDescending(d => d.Debt)
            .ToList();
        
        var creditors = balances
            .Where(b => b.Balance > 0)
            .Select(b => new { b.UserId, b.UserName, Credit = b.Balance })
            .OrderByDescending(c => c.Credit)
            .ToList();
        
        foreach (var debtor in debtors)
        {
            decimal remainingDebt = debtor.Debt;
            
            foreach (var creditor in creditors.Where(c => c.Credit > 0).ToList())
            {
                if (remainingDebt <= 0) break;
                
                decimal paymentAmount = Math.Min(remainingDebt, creditor.Credit);
                
                if (paymentAmount > 0)
                {
                    debts.Add(new DebtDto
                    {
                        FromUserId = debtor.UserId,
                        FromUserName = debtor.UserName,
                        ToUserId = creditor.UserId,
                        ToUserName = creditor.UserName,
                        Amount = paymentAmount
                    });
                    
                    remainingDebt -= paymentAmount;
                    var creditorIndex = creditors.FindIndex(c => c.UserId == creditor.UserId);
                    if (creditorIndex >= 0)
                    {
                        creditors[creditorIndex] = new { creditor.UserId, creditor.UserName, Credit = creditor.Credit - paymentAmount };
                    }
                }
            }
        }
        
        return debts;
    }

    public async Task<UserDebtsDto?> GetUserDebtsAsync(Guid tripId, Guid userId)
    {
        var allDebts = await GetTripDebtsAsync(tripId);
        var balances = await GetTripBalanceAsync(tripId);
        var userBalance = balances.FirstOrDefault(b => b.UserId == userId);
        
        if (userBalance == null)
        {
            return null;
        }
        
        var userDebts = allDebts
            .Where(d => d.FromUserId == userId)
            .ToList();
        
        var userCredits = allDebts
            .Where(d => d.ToUserId == userId)
            .ToList();
        
        return new UserDebtsDto
        {
            UserId = userId,
            UserName = userBalance.UserName,
            TotalDebt = userBalance.Balance < 0 ? Math.Abs(userBalance.Balance) : 0,
            TotalCredit = userBalance.Balance > 0 ? userBalance.Balance : 0,
            Debts = userDebts,
            Credits = userCredits
        };
    }

    private TripDto MapToDto(Trip trip)
    {
        var tripDto = new TripDto
        {
            Id = trip.Id,
            Name = trip.Name,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            OrganizerId = trip.OrganizerId,
            OrganizerName = trip.Organizer?.Name ?? "",
            IsCompleted = trip.IsCompleted,
            ParticipantIds = new List<Guid>()
        };

        foreach (var participant in trip.Participants)
        {
            tripDto.ParticipantIds.Add(participant.UserId);
        }

        return tripDto;
    }

    private async Task InvalidateCacheAsync(Guid tripId)
    {
        var cacheKey = CacheKeyPrefix + tripId.ToString();
        await _cache.RemoveAsync(cacheKey);
    }
}
