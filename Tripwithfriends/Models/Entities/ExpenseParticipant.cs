namespace Tripwithfriends.Models.Entities;

public class ExpenseParticipant
{
    public Guid ExpenseId { get; set; }
    public Expense Expense { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}


