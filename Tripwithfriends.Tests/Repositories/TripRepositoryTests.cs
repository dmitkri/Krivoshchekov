using Microsoft.EntityFrameworkCore;
using Tripwithfriends.Data;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories;

namespace Tripwithfriends.Tests.Repositories;

public class TripRepositoryTests
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
            PasswordHash = "SecurePassword123!",
            Role = "User"
        };
    }

    [Fact]
    public async Task AddAsync_ShouldCreateTrip()
    {
        using var context = GetDbContext();
        var repository = new TripRepository(context);
        
        var organizer = CreateTestUser(Guid.NewGuid(), "Наполеон Бонапарт", "napoleon@versailles.fr");
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка в Париж",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            OrganizerId = organizer.Id,
            IsCompleted = false
        };

        var result = await repository.AddAsync(trip);

        Assert.NotNull(result);
        Assert.Equal(trip.Id, result.Id);
        Assert.Equal("Поездка в Париж", result.Name);
        
        var savedTrip = await context.Trips.FindAsync(trip.Id);
        Assert.NotNull(savedTrip);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTrip_WhenTripExists()
    {
        using var context = GetDbContext();
        var repository = new TripRepository(context);
        
        var organizer = CreateTestUser(Guid.NewGuid(), "Юлий Цезарь", "caesar@rome.empire");
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            OrganizerId = organizer.Id,
            IsCompleted = false
        };
        await repository.AddAsync(trip);

        var result = await repository.GetByIdAsync(trip.Id);

        Assert.NotNull(result);
        Assert.Equal(trip.Id, result.Id);
        Assert.Equal("Поездка", result.Name);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnTripWithDetails()
    {
        using var context = GetDbContext();
        var repository = new TripRepository(context);
        
        var organizer = CreateTestUser(Guid.NewGuid(), "Александр Македонский", "alexander@macedon.ancient");
        var participant = CreateTestUser(Guid.NewGuid(), "Аристотель", "aristotle@lyceum.greece");
        context.Users.Add(organizer);
        context.Users.Add(participant);
        await context.SaveChangesAsync();
        
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка с деталями",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3),
            OrganizerId = organizer.Id,
            IsCompleted = false
        };
        
        trip.Participants.Add(new TripParticipant
        {
            TripId = trip.Id,
            UserId = participant.Id
        });
        
        await repository.AddAsync(trip);

        var result = await repository.GetByIdWithDetailsAsync(trip.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.Organizer);
        Assert.Equal(organizer.Name, result.Organizer.Name);
        Assert.Single(result.Participants);
        Assert.Equal(participant.Id, result.Participants.First().UserId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTrip()
    {
        using var context = GetDbContext();
        var repository = new TripRepository(context);
        
        var organizer = CreateTestUser(Guid.NewGuid(), "Пётр I", "peter@great.russia");
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Старая поездка",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            OrganizerId = organizer.Id,
            IsCompleted = false
        };
        await repository.AddAsync(trip);

        trip.Name = "Новая поездка";
        trip.IsCompleted = true;
        await repository.UpdateAsync(trip);

        var updatedTrip = await repository.GetByIdAsync(trip.Id);
        Assert.NotNull(updatedTrip);
        Assert.Equal("Новая поездка", updatedTrip.Name);
        Assert.True(updatedTrip.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTrip()
    {
        using var context = GetDbContext();
        var repository = new TripRepository(context);
        
        var organizer = CreateTestUser(Guid.NewGuid(), "Клеопатра", "cleopatra@egypt.queen");
        context.Users.Add(organizer);
        await context.SaveChangesAsync();
        
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка для удаления",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            OrganizerId = organizer.Id,
            IsCompleted = false
        };
        await repository.AddAsync(trip);

        await repository.DeleteAsync(trip);

        var deletedTrip = await repository.GetByIdAsync(trip.Id);
        Assert.Null(deletedTrip);
    }

    [Fact]
    public async Task GetByOrganizerIdAsync_ShouldReturnTripsByOrganizer()
    {
        using var context = GetDbContext();
        var repository = new TripRepository(context);
        
        var organizer1 = CreateTestUser(Guid.NewGuid(), "Галилео Галилей", "galileo@padua.university");
        var organizer2 = CreateTestUser(Guid.NewGuid(), "Микеланджело", "michelangelo@vatican.art");
        context.Users.Add(organizer1);
        context.Users.Add(organizer2);
        await context.SaveChangesAsync();
        
        var trip1 = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка 1",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            OrganizerId = organizer1.Id,
            IsCompleted = false
        };
        
        var trip2 = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка 2",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3),
            OrganizerId = organizer1.Id,
            IsCompleted = false
        };
        
        var trip3 = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка 3",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            OrganizerId = organizer2.Id,
            IsCompleted = false
        };
        
        await repository.AddAsync(trip1);
        await repository.AddAsync(trip2);
        await repository.AddAsync(trip3);

        var result = await repository.GetByOrganizerIdAsync(organizer1.Id);

        Assert.NotNull(result);
        var tripsList = result.ToList();
        Assert.Equal(2, tripsList.Count);
        Assert.All(tripsList, t => Assert.Equal(organizer1.Id, t.OrganizerId));
    }

    [Fact]
    public async Task GetByParticipantIdAsync_ShouldReturnTripsByParticipant()
    {
        using var context = GetDbContext();
        var repository = new TripRepository(context);
        
        var organizer = CreateTestUser(Guid.NewGuid(), "Сократ", "socrates@athens.philosophy");
        var participant = CreateTestUser(Guid.NewGuid(), "Платон", "plato@academy.greece");
        context.Users.Add(organizer);
        context.Users.Add(participant);
        await context.SaveChangesAsync();
        
        var trip1 = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка 1",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            OrganizerId = organizer.Id,
            IsCompleted = false
        };
        trip1.Participants.Add(new TripParticipant { TripId = trip1.Id, UserId = participant.Id });
        
        var trip2 = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка 2",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3),
            OrganizerId = organizer.Id,
            IsCompleted = false
        };
        trip2.Participants.Add(new TripParticipant { TripId = trip2.Id, UserId = participant.Id });
        
        var trip3 = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Поездка 3",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            OrganizerId = organizer.Id,
            IsCompleted = false
        };
        
        await repository.AddAsync(trip1);
        await repository.AddAsync(trip2);
        await repository.AddAsync(trip3);

        var result = await repository.GetByParticipantIdAsync(participant.Id);

        Assert.NotNull(result);
        var tripsList = result.ToList();
        Assert.Equal(2, tripsList.Count);
        Assert.All(tripsList, t => Assert.Contains(t.Participants, p => p.UserId == participant.Id));
    }
}


