using Tripwithfriends.Models.Entities;

namespace Tripwithfriends.Repositories.Interfaces;

public interface ITripRepository : IRepository<Trip>
{
    Task<Trip?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Trip>> GetByOrganizerIdAsync(Guid organizerId);
    Task<IEnumerable<Trip>> GetByParticipantIdAsync(Guid participantId);
    Task<IEnumerable<Trip>> GetAllWithDetailsAsync();
}

