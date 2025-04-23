using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rooms.API.Entities
{
    /// <summary>
    /// Represents the status of a room in the system.
    /// </summary>
    public enum RoomStatus
    {
        /// <summary>
        /// The room is available for use.
        /// </summary>
        Available,

        /// <summary>
        /// The room is currently occupied.
        /// </summary>
        Occupied,

        /// <summary>
        /// The room is under maintenance and not available for use.
        /// </summary>
        Maintenance,
    }

    public class Room
    {
        public Guid Id { get; set; }

        public Guid BlockId { get; set; }

        public string Label { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public RoomStatus Status { get; set; } // Enum for status

        public Collection<string> Amenities { get; init; } = new Collection<string>();
    }
}