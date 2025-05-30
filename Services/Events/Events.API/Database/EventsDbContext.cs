using Events.API.Entities;

using Microsoft.EntityFrameworkCore;

namespace Events.API.Database
{
    public sealed class EventsDbContext : DbContext
    {
        public EventsDbContext(DbContextOptions<EventsDbContext> options)
            : base(options)
        {
        }

        public DbSet<DormEvent> Events { get; set; } = null!;

        public DbSet<EventParticipant> EventParticipants { get; set; } = null!;

        public DbSet<InvitationToken> InvitationTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.Entity<EventParticipant>()
                .HasKey(ep => ep.Id);

            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.EventId);

            modelBuilder.Entity<InvitationToken>()
                .HasOne(i => i.Event)
                .WithMany()
                .HasForeignKey(i => i.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Make token unique
            modelBuilder.Entity<InvitationToken>()
                .HasIndex(i => i.Token)
                .IsUnique();

            // Seed data
            SeedData.SeedEvents(modelBuilder);
        }
    }
}