using Microsoft.EntityFrameworkCore;

using TelegramAgent.API.Entities;

namespace TelegramAgent.API.Data
{
    public class TelegramDbContext : DbContext
    {
        public DbSet<TelegramLink> TelegramLinks { get; set; } = null!;

        public TelegramDbContext(DbContextOptions<TelegramDbContext> options)
            : base(options)
        {
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