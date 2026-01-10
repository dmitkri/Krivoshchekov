namespace Tripwithfriends.Models.Entities;

public class TripParticipant
{
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}


