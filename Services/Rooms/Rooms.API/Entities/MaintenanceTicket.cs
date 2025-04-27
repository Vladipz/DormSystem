namespace Rooms.API.Entities
{
    public enum MaintenanceStatus
    {
        /// <summary>
        /// The maintenance ticket is open and has not been addressed yet.
        /// </summary>
        Open,

        /// <summary>
        /// The maintenance ticket is currently being worked on.
        /// </summary>
        InProgress,

        /// <summary>
        /// The maintenance ticket has been resolved.
        /// </summary>
        Resolved,
    }

    public class MaintenanceTicket
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsResolved { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Open; // <<<< ADD THIS FIELD

        /// <summary>
        /// Gets or sets navigation property for the room this maintenance ticket is related to.
        /// </summary>
        public Room Room { get; set; } = null!;
    }
}