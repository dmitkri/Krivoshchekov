namespace Tripwithfriends.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public List<Trip> OrganizedTrips { get; set; } = new();
    public List<TripParticipant> Participations { get; set; } = new();
    public List<Expense> PaidExpenses { get; set; } = new();
    public List<ExpenseParticipant> ExpenseParticipations { get; set; } = new();
}


