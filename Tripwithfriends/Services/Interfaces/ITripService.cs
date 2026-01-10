using Tripwithfriends.DTO;

namespace Tripwithfriends.Services.Interfaces;

public interface ITripService
{
    Task<TripDto> CreateTripAsync(CreateTripDto createTripDto, Guid organizerId);
    Task<TripDto?> GetTripByIdAsync(Guid id);
    Task<IEnumerable<TripDto>> GetAllTripsAsync();
    Task<TripDto> UpdateTripAsync(Guid id, UpdateTripDto updateTripDto, Guid userId, string userRole);
    Task DeleteTripAsync(Guid id, Guid userId, string userRole);
    Task CompleteTripAsync(Guid id, Guid userId, string userRole);
    Task<List<TripBalanceDto>> GetTripBalanceAsync(Guid tripId);
    Task<List<DebtDto>> GetTripDebtsAsync(Guid tripId);
    Task<UserDebtsDto?> GetUserDebtsAsync(Guid tripId, Guid userId);
}

