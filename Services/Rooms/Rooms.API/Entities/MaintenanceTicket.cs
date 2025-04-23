using System;

namespace Rooms.API.Entities
{
    /// <summary>
    /// Represents the status of a maintenance ticket.
    /// </summary>
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

        public string Description { get; set; } = string.Empty;

        public MaintenanceStatus Status { get; set; } // Enum for status

        public Guid ReportedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}