using Rooms.API.Entities;

namespace Rooms.API.Contracts.Maintenance
{
    public sealed class CreateMaintenanceTicketRequest
    {
        public Guid RoomId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid ReporterById { get; set; }

        public Guid? AssignedToId { get; set; }

        public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;
    }
}