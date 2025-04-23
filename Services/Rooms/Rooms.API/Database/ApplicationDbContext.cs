using Microsoft.EntityFrameworkCore;

using Rooms.API.Entities;

namespace Rooms.API.Database
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
    }
}
