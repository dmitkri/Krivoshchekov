namespace Tripwithfriends.Models.Entities;

public class Trip
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;
    public List<TripParticipant> Participants { get; set; } = new();
    public List<Expense> Expenses { get; set; } = new();
    public bool IsCompleted { get; set; }
}


