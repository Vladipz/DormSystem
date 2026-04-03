using Microsoft.EntityFrameworkCore;

using TelegramAgent.API.Entities;

namespace TelegramAgent.API.Data
{
    public class TelegramDbContext : DbContext
    {
        public DbSet<TelegramLink> TelegramLinks { get; set; } = null!;

        // Parameterless constructor for EF Core design-time tools
        public TelegramDbContext()
        {
        }

        public TelegramDbContext(DbContextOptions<TelegramDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // This is only used at design-time for migrations
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=telegram-db;Username=postgres;Password=postgres");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TelegramLink>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.ChatId).IsRequired();
                entity.Property(e => e.LinkedAt).IsRequired();
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.ChatId).IsUnique();
            });
        }
    }
}