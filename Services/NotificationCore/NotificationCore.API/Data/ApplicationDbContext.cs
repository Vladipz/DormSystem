using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Entities;

namespace NotificationCore.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications => Set<Notification>();

        public DbSet<UserNotificationSetting> UserNotificationSettings => Set<UserNotificationSetting>();

        public DbSet<UserChannel> UserChannels => Set<UserChannel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserNotificationSetting>()
                .Property(x => x.NotificationType)
                .HasConversion<string>();

            modelBuilder.Entity<UserChannel>()
                .Property(x => x.Channel)
                .HasConversion<string>();

            modelBuilder.Entity<Notification>()
            .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder.Entity<UserNotificationSetting>()
                .HasIndex(x => new { x.UserId, x.NotificationType })
                .IsUnique();

            modelBuilder.Entity<UserChannel>()
                .HasIndex(x => new { x.UserId, x.Channel })
                .IsUnique();
        }
    }
}