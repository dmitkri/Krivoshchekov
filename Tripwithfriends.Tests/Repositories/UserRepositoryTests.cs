using Microsoft.EntityFrameworkCore;
using Tripwithfriends.Data;
using Tripwithfriends.Models.Entities;
using Tripwithfriends.Repositories;

namespace Tripwithfriends.Tests.Repositories;

public class UserRepositoryTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateUser()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Альберт Эйнштейн",
            Email = "einstein@princeton.edu",
            PasswordHash = "E=mc2!1905",
            Role = "User"
        };

        var result = await repository.AddAsync(user);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("Альберт Эйнштейн", result.Name);
        Assert.Equal("einstein@princeton.edu", result.Email);
        
        var savedUser = await context.Users.FindAsync(user.Id);
        Assert.NotNull(savedUser);
        Assert.Equal(user.Email, savedUser.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Лев Толстой",
            Email = "tolstoy@yasnaya-polyana.ru",
            PasswordHash = "War&Peace1869",
            Role = "User"
        };
        await repository.AddAsync(user);

        var result = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);
        
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Name = "Вольфганг Амадей Моцарт",
            Email = "mozart@salsburg.music",
            PasswordHash = "Requiem1791",
            Role = "User"
        };
        
        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Name = "Стив Джобс",
            Email = "jobs@apple.com",
            PasswordHash = "ThinkDifferent1997",
            Role = "Admin"
        };
        
        await repository.AddAsync(user1);
        await repository.AddAsync(user2);

        var result = await repository.GetAllAsync();

        Assert.NotNull(result);
        var usersList = result.ToList();
        Assert.Equal(2, usersList.Count);
        Assert.Contains(usersList, u => u.Email == "mozart@salsburg.music");
        Assert.Contains(usersList, u => u.Email == "jobs@apple.com");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Исаак Ньютон",
            Email = "newton@cambridge.edu",
            PasswordHash = "Gravity1687",
            Role = "User"
        };
        await repository.AddAsync(user);

        user.Name = "Мария Кюри";
        user.Email = "curie@sorbonne.fr";
        await repository.UpdateAsync(user);

        var updatedUser = await repository.GetByIdAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("Мария Кюри", updatedUser.Name);
        Assert.Equal("curie@sorbonne.fr", updatedUser.Email);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Никола Тесла",
            Email = "tesla@westinghouse.com",
            PasswordHash = "AC1888",
            Role = "User"
        };
        await repository.AddAsync(user);

        await repository.DeleteAsync(user);

        var deletedUser = await repository.GetByIdAsync(user.Id);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Фёдор Достоевский",
            Email = "dostoevsky@russian-literature.ru",
            PasswordHash = "Crime&Punish1866",
            Role = "User"
        };
        await repository.AddAsync(user);

        var result = await repository.GetByEmailAsync("dostoevsky@russian-literature.ru");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("dostoevsky@russian-literature.ru", result.Email);
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Леонардо да Винчи",
            Email = "vinci@florence.renaissance",
            PasswordHash = "MonaLisa1503",
            Role = "User"
        };
        await repository.AddAsync(user);

        var exists = await repository.EmailExistsAsync("vinci@florence.renaissance");

        Assert.True(exists);
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);

        var exists = await repository.EmailExistsAsync("shakespeare@globe.theatre");

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenUserExists()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Винсент ван Гог",
            Email = "vangogh@arles.art",
            PasswordHash = "StarryNight1889",
            Role = "User"
        };
        await repository.AddAsync(user);

        var exists = await repository.ExistsAsync(user.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        using var context = GetDbContext();
        var repository = new UserRepository(context);

        var exists = await repository.ExistsAsync(Guid.NewGuid());

        Assert.False(exists);
    }
}


