using Rooms.API.Contracts.Room;
using Rooms.API.Entities;

using Shared.UserServiceClient;

namespace Rooms.API.Contracts.Maintenance
{
    public sealed class MaintenanceTicketResponse
    {
        public Guid Id { get; set; }

        // Remove RoomId and replace with Room object
        public ShortRoomResponse Room { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public MaintenanceStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public UserDto Reporter { get; set; } = null!;

        public UserDto? AssignedTo { get; set; } = null!;

        public MaintenancePriority Priority { get; set; }
    }
}