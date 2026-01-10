using Microsoft.EntityFrameworkCore;
using Tripwithfriends.Models.Entities;

namespace Tripwithfriends.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<TripParticipant> TripParticipants { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ExpenseParticipant> ExpenseParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Organizer)
                .WithMany(u => u.OrganizedTrips)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TripParticipant>(entity =>
        {
            entity.HasKey(e => new { e.TripId, e.UserId });
            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Participants)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Participations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Payer)
                .WithMany(u => u.PaidExpenses)
                .HasForeignKey(e => e.PayerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExpenseParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ExpenseId, e.UserId });
            entity.HasOne(e => e.Expense)
                .WithMany(exp => exp.Participants)
                .HasForeignKey(e => e.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(u => u.ExpenseParticipations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}


