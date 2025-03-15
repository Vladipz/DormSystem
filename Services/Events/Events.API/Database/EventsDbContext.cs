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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.Entity<EventParticipant>()
                .HasKey(ep => ep.Id);

            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.EventId);
        }
    }
}