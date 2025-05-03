using Microsoft.EntityFrameworkCore;

using Rooms.API.Entities;

namespace Rooms.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Floor> Floors { get; set; }

        public DbSet<Block> Blocks { get; set; }

        public DbSet<Room> Rooms { get; set; }

        public DbSet<Place> Places { get; set; }

        public DbSet<MaintenanceTicket> MaintenanceTickets { get; set; }

        public DbSet<Building> Buildings { get; set; }
    }
}