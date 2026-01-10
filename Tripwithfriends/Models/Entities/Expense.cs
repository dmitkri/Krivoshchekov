namespace Tripwithfriends.Models.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid PayerId { get; set; }
    public User Payer { get; set; } = null!;
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    public List<ExpenseParticipant> Participants { get; set; } = new();
}


