using Microsoft.EntityFrameworkCore;
using Tripwithfriends.Data;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories.Interfaces;

namespace Tripwithfriends.Repositories;

public class TripRepository : Repository<Trip>, ITripRepository
{
    public TripRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Trip?> GetByIdWithDetailsAsync(Guid id)
    {
        var trip = await _dbSet
            .Include(t => t.Organizer)
            .Include(t => t.Participants)
                .ThenInclude(tp => tp.User)
            .Include(t => t.Expenses)
                .ThenInclude(e => e.Payer)
            .Include(t => t.Expenses)
                .ThenInclude(e => e.Participants)
                    .ThenInclude(ep => ep.User)
            .FirstOrDefaultAsync(t => t.Id == id);
        
        return trip;
    }

    public async Task<IEnumerable<Trip>> GetByOrganizerIdAsync(Guid organizerId)
    {
        var trips = await _dbSet
            .Include(t => t.Organizer)
            .Include(t => t.Participants)
                .ThenInclude(tp => tp.User)
            .Where(t => t.OrganizerId == organizerId)
            .ToListAsync();
        
        return trips;
    }

    public async Task<IEnumerable<Trip>> GetByParticipantIdAsync(Guid participantId)
    {
        var trips = await _dbSet
            .Include(t => t.Participants)
            .Where(t => t.Participants.Any(p => p.UserId == participantId))
            .ToListAsync();
        
        return trips;
    }

    public async Task<IEnumerable<Trip>> GetAllWithDetailsAsync()
    {
        var trips = await _dbSet
            .Include(t => t.Organizer)
            .Include(t => t.Participants)
                .ThenInclude(tp => tp.User)
            .ToListAsync();
        
        return trips;
    }
}
