namespace Tripwithfriends.DTO;

public class TripDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}

public class CreateTripDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}

public class UpdateTripDto
{
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<Guid>? ParticipantIds { get; set; }
}

public class TripBalanceDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal OwedAmount { get; set; }
    public decimal Balance { get; set; }
}

public class DebtDto
{
    public Guid FromUserId { get; set; }
    public string FromUserName { get; set; } = string.Empty;
    public Guid ToUserId { get; set; }
    public string ToUserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class UserDebtsDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalDebt { get; set; }
    public decimal TotalCredit { get; set; }
    public List<DebtDto> Debts { get; set; } = new();
    public List<DebtDto> Credits { get; set; } = new();
}

