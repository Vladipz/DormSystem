using Rooms.API.Entities;

namespace Rooms.API.Contracts.Maintenance
{
    public sealed class UpdateMaintenanceTicketRequest
    {
        public Guid TicketId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid? AssignedToId { get; set; }

        public MaintenancePriority Priority { get; set; }
    }
}