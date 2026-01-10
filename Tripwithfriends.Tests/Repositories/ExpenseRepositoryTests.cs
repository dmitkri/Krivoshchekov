using Microsoft.EntityFrameworkCore;
using Tripwithfriends.Data;
using Tripwithfriends.DTO;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories;

namespace Tripwithfriends.Tests.Repositories;

public class ExpenseRepositoryTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApplicationDbContext(options);
    }

    private User CreateTestUser(Guid id, string name, string email)
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = email,
            PasswordHash = "SecurePassword456!",
            Role = "User"
        };
    }

    private Trip CreateTestTrip(Guid id, Guid organizerId, string name)
    {
        return new Trip
        {
            Id = id,
            Name = name,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            OrganizerId = organizerId,
            IsCompleted = false
        };
    }

    [Fact]
    public async Task AddAsync_ShouldCreateExpense()
    {
        using var context = GetDbContext();
        var repository = new ExpenseRepository(context);
        
        var payer = CreateTestUser(Guid.NewGuid(), "Билл Гейтс", "gates@microsoft.com");
        var organizer = CreateTestUser(Guid.NewGuid(), "Уоррен Баффет", "buffett@berkshire.com");
        context.Users.Add(payer);
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = CreateTestTrip(Guid.NewGuid(), organizer.Id, "Поездка");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Обед",
            Amount = 1000.50m,
            Category = "Еда",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip.Id
        };

        var result = await repository.AddAsync(expense);

        Assert.NotNull(result);
        Assert.Equal(expense.Id, result.Id);
        Assert.Equal("Обед", result.Name);
        Assert.Equal(1000.50m, result.Amount);
        
        var savedExpense = await context.Expenses.FindAsync(expense.Id);
        Assert.NotNull(savedExpense);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnExpense_WhenExpenseExists()
    {
        using var context = GetDbContext();
        var repository = new ExpenseRepository(context);
        
        var payer = CreateTestUser(Guid.NewGuid(), "Илон Маск", "musk@tesla.com");
        var organizer = CreateTestUser(Guid.NewGuid(), "Джефф Безос", "bezos@amazon.com");
        context.Users.Add(payer);
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = CreateTestTrip(Guid.NewGuid(), organizer.Id, "Поездка");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Расход",
            Amount = 500m,
            Category = "Транспорт",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip.Id
        };
        await repository.AddAsync(expense);
        var result = await repository.GetByIdAsync(expense.Id);

        Assert.NotNull(result);
        Assert.Equal(expense.Id, result.Id);
        Assert.Equal("Расход", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExpense()
    {
        using var context = GetDbContext();
        var repository = new ExpenseRepository(context);
        
        var payer = CreateTestUser(Guid.NewGuid(), "Марк Цукерберг", "zuckerberg@meta.com");
        var organizer = CreateTestUser(Guid.NewGuid(), "Ларри Пейдж", "page@google.com");
        context.Users.Add(payer);
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = CreateTestTrip(Guid.NewGuid(), organizer.Id, "Поездка");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Старый расход",
            Amount = 100m,
            Category = "Старая категория",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip.Id
        };
        await repository.AddAsync(expense);

        expense.Name = "Новый расход";
        expense.Amount = 200m;
        expense.Category = "Новая категория";
        await repository.UpdateAsync(expense);

        var updatedExpense = await repository.GetByIdAsync(expense.Id);
        Assert.NotNull(updatedExpense);
        Assert.Equal("Новый расход", updatedExpense.Name);
        Assert.Equal(200m, updatedExpense.Amount);
        Assert.Equal("Новая категория", updatedExpense.Category);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteExpense()
    {
        using var context = GetDbContext();
        var repository = new ExpenseRepository(context);
        
        var payer = CreateTestUser(Guid.NewGuid(), "Сергей Брин", "brin@google.com");
        var organizer = CreateTestUser(Guid.NewGuid(), "Тим Кук", "cook@apple.com");
        context.Users.Add(payer);
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = CreateTestTrip(Guid.NewGuid(), organizer.Id, "Поездка");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Расход для удаления",
            Amount = 300m,
            Category = "Категория",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip.Id
        };
        await repository.AddAsync(expense);
        await repository.DeleteAsync(expense);

        var deletedExpense = await repository.GetByIdAsync(expense.Id);
        Assert.Null(deletedExpense);
    }

    [Fact]
    public async Task GetByTripIdAsync_ShouldReturnExpensesForTrip()
    {
        using var context = GetDbContext();
        var repository = new ExpenseRepository(context);
        
        var payer = CreateTestUser(Guid.NewGuid(), "Ричард Брэнсон", "branson@virgin.com");
        var organizer = CreateTestUser(Guid.NewGuid(), "Рид Хоффман", "hoffman@linkedin.com");
        context.Users.Add(payer);
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip1 = CreateTestTrip(Guid.NewGuid(), organizer.Id, "Поездка 1");
        var trip2 = CreateTestTrip(Guid.NewGuid(), organizer.Id, "Поездка 2");
        context.Trips.Add(trip1);
        context.Trips.Add(trip2);
        await context.SaveChangesAsync();
        
        var expense1 = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Расход 1",
            Amount = 100m,
            Category = "Категория",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip1.Id
        };
        
        var expense2 = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Расход 2",
            Amount = 200m,
            Category = "Категория",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip1.Id
        };
        
        var expense3 = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Расход 3",
            Amount = 300m,
            Category = "Категория",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip2.Id
        };
        
        await repository.AddAsync(expense1);
        await repository.AddAsync(expense2);
        await repository.AddAsync(expense3);

        var result = await repository.GetByTripIdAsync(trip1.Id);

        Assert.NotNull(result);
        var expensesList = result.ToList();
        Assert.Equal(2, expensesList.Count);
        Assert.All(expensesList, e => Assert.Equal(trip1.Id, e.TripId));
    }

    [Fact]
    public async Task GetFilteredAsync_ShouldFilterExpenses()
    {
        using var context = GetDbContext();
        var repository = new ExpenseRepository(context);
        
        var payer = CreateTestUser(Guid.NewGuid(), "Джек Ма", "jack@alibaba.com");
        var organizer = CreateTestUser(Guid.NewGuid(), "Сатья Наделла", "nadella@microsoft.com");
        context.Users.Add(payer);
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = CreateTestTrip(Guid.NewGuid(), organizer.Id, "Поездка");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        
        var expense1 = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Обед в ресторане",
            Amount = 1000m,
            Category = "Еда",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip.Id
        };
        
        var expense2 = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Такси",
            Amount = 500m,
            Category = "Транспорт",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip.Id
        };
        
        var expense3 = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Завтрак",
            Amount = 300m,
            Category = "Еда",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip.Id
        };
        
        await repository.AddAsync(expense1);
        await repository.AddAsync(expense2);
        await repository.AddAsync(expense3);

        var filter = new ExpenseFilterDto
        {
            Page = 1,
            PageSize = 10,
            Category = "Еда",
            TripId = trip.Id
        };
        var result = await repository.GetFilteredAsync(filter);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, e => Assert.Equal("Еда", e.Category));
    }

    [Fact]
    public async Task GetFilteredAsync_ShouldPaginateExpenses()
    {
        using var context = GetDbContext();
        var repository = new ExpenseRepository(context);
        
        var payer = CreateTestUser(Guid.NewGuid(), "Рэй Далио", "dalio@bridgewater.com");
        var organizer = CreateTestUser(Guid.NewGuid(), "Чарльз Мангер", "munger@berkshire.com");
        context.Users.Add(payer);
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = CreateTestTrip(Guid.NewGuid(), organizer.Id, "Поездка");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        
        for (int i = 1; i <= 5; i++)
        {
            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                Name = $"Расход {i}",
                Amount = 100m * i,
                Category = "Категория",
                Date = DateTime.UtcNow.AddDays(-i),
                PayerId = payer.Id,
                TripId = trip.Id
            };
            await repository.AddAsync(expense);
        }

        var filter = new ExpenseFilterDto
        {
            Page = 1,
            PageSize = 2,
            TripId = trip.Id
        };
        var result = await repository.GetFilteredAsync(filter);

        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetTotalPaidByUserAsync_ShouldReturnTotalPaid()
    {
        using var context = GetDbContext();
        var repository = new ExpenseRepository(context);
        
        var payer = CreateTestUser(Guid.NewGuid(), "Джордж Сорос", "soros@quantumfund.com");
        var organizer = CreateTestUser(Guid.NewGuid(), "Карл Айкан", "icahn@icahn.com");
        context.Users.Add(payer);
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = CreateTestTrip(Guid.NewGuid(), organizer.Id, "Поездка");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        
        var expense1 = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Расход 1",
            Amount = 1000m,
            Category = "Категория",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip.Id
        };
        
        var expense2 = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Расход 2",
            Amount = 500m,
            Category = "Категория",
            Date = DateTime.UtcNow,
            PayerId = payer.Id,
            TripId = trip.Id
        };
        
        await repository.AddAsync(expense1);
        await repository.AddAsync(expense2);
        var totalPaid = await repository.GetTotalPaidByUserAsync(payer.Id, trip.Id);

        Assert.Equal(1500m, totalPaid);
    }
}


