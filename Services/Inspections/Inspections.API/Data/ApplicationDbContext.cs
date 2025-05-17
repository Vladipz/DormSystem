using Inspections.API.Entities;

using Microsoft.EntityFrameworkCore;

namespace Inspections.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inspection> Inspections { get; set; } = null!;

        public DbSet<RoomInspection> RoomInspections { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            // Inspection → RoomInspection: один-до-багатьох
            modelBuilder.Entity<Inspection>()
                .HasMany(i => i.Rooms)
                .WithOne(r => r.Inspection)
                .HasForeignKey(r => r.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // RoomInspection: RoomId — це plain value (НЕ зовнішній ключ)
            modelBuilder.Entity<RoomInspection>()
                .Property(r => r.RoomId)
                .IsRequired();

            modelBuilder.Entity<RoomInspection>()
                .Property(r => r.RoomNumber)
                .HasMaxLength(32);

            modelBuilder.Entity<RoomInspection>()
                .Property(r => r.Floor)
                .HasMaxLength(16);

            modelBuilder.Entity<RoomInspection>()
                .Property(r => r.Building)
                .HasMaxLength(32);

            // Enum-конверсії для зручного зберігання у вигляді string
            modelBuilder.Entity<Inspection>()
                .Property(i => i.Status)
                .HasConversion<string>();

            modelBuilder.Entity<RoomInspection>()
                .Property(r => r.Status)
                .HasConversion<string>();
        }
    }
}
