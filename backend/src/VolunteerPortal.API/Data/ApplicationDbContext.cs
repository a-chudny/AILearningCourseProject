using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Models.Entities;

namespace VolunteerPortal.API.Data;

/// <summary>
/// Entity Framework Core database context for the Volunteer Portal application.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<EventSkill> EventSkills => Set<EventSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter for soft deletes on User and Event entities
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Name).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.Role).IsRequired().HasConversion<int>();
            entity.Property(u => u.CreatedAt).IsRequired();
            entity.Property(u => u.IsDeleted).IsRequired().HasDefaultValue(false);

            // One-to-many: User -> Events (as Organizer)
            entity.HasMany(u => u.OrganizedEvents)
                .WithOne(e => e.Organizer)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // One-to-many: User -> Registrations
            entity.HasMany(u => u.Registrations)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Event entity configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.Capacity).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasConversion<int>();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            // One-to-many: Event -> Registrations
            entity.HasMany(e => e.Registrations)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Registration entity configuration
        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(r => r.Id);

            // Composite unique index: One user can only register once per event
            entity.HasIndex(r => new { r.EventId, r.UserId }).IsUnique();

            entity.Property(r => r.Status).IsRequired().HasConversion<int>();
            entity.Property(r => r.RegisteredAt).IsRequired();
            entity.Property(r => r.Notes).HasMaxLength(1000);
        });

        // Skill entity configuration
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => s.Name).IsUnique();

            entity.Property(s => s.Name).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.Property(s => s.CreatedAt).IsRequired();
        });

        // UserSkill join entity configuration (many-to-many: User <-> Skill)
        modelBuilder.Entity<UserSkill>(entity =>
        {
            // Composite primary key
            entity.HasKey(us => new { us.UserId, us.SkillId });

            entity.Property(us => us.ProficiencyLevel).HasMaxLength(100);
            entity.Property(us => us.AddedAt).IsRequired();

            // Many-to-many: User <-> Skill
            entity.HasOne(us => us.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EventSkill join entity configuration (many-to-many: Event <-> Skill)
        modelBuilder.Entity<EventSkill>(entity =>
        {
            // Composite primary key
            entity.HasKey(es => new { es.EventId, es.SkillId });

            entity.Property(es => es.IsRequired).IsRequired();
            entity.Property(es => es.AddedAt).IsRequired();

            // Many-to-many: Event <-> Skill
            entity.HasOne(es => es.Event)
                .WithMany(e => e.EventSkills)
                .HasForeignKey(es => es.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(es => es.Skill)
                .WithMany(s => s.EventSkills)
                .HasForeignKey(es => es.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
