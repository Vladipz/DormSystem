using Rooms.API.Entities;

namespace Rooms.API.Contracts.Maintenance
{
    public sealed class MaintenanceTicketDetailsResponse
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public MaintenanceStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}