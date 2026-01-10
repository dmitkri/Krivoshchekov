namespace Tripwithfriends.DTO;

public class ExpenseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid PayerId { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public Guid TripId { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}

public class CreateExpenseDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid PayerId { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}

public class UpdateExpenseDto
{
    public string? Name { get; set; }
    public decimal? Amount { get; set; }
    public string? Category { get; set; }
    public DateTime? Date { get; set; }
    public Guid? PayerId { get; set; }
    public List<Guid>? ParticipantIds { get; set; }
}

public class ExpenseFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Category { get; set; }
    public Guid? TripId { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}


