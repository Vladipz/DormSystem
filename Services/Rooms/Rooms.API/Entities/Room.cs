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

    /// <summary>
    /// Represents the type of room in the system.
    /// </summary>
    public enum RoomType
    {
        /// <summary>
        /// Standard dormitory room for residents.
        /// </summary>
        Regular,

        /// <summary>
        /// Room for specific purposes (study, laundry, etc.).
        /// </summary>
        Specialized,
    }

    public class Room
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets optional reference to the block this room belongs to.
        /// Null for event rooms or specialized rooms not within blocks.
        /// </summary>
        public Guid? BlockId { get; set; }

        public Guid? FloorId { get; set; }

        public string Label { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public RoomStatus Status { get; set; }

        /// <summary>
        /// Gets or sets type of room (Regular, Event, Specialized).
        /// </summary>
        public RoomType RoomType { get; set; } = RoomType.Regular;

        /// <summary>
        /// Gets or sets purpose description for specialized or event rooms.
        /// </summary>
        public string? Purpose { get; set; }

        public Collection<string> Amenities { get; init; } = new Collection<string>();

        /// <summary>
        /// Gets or sets navigation property for the block this room belongs to.
        /// Null for event rooms or specialized rooms not within blocks.
        /// </summary>
        public Block? Block { get; set; }

        /// <summary>
        /// Gets or sets navigation property for the floor this room belongs to.
        /// </summary>
        public Floor? Floor { get; set; }

        /// <summary>
        /// Gets navigation property for the places in this room.
        /// Empty for event rooms or specialized rooms.
        /// </summary>
        public ICollection<Place> Places { get; init; } = new List<Place>();

        /// <summary>
        /// Gets navigation property for maintenance tickets related to this room.
        /// </summary>
        public ICollection<MaintenanceTicket> MaintenanceTickets { get; init; } = new List<MaintenanceTicket>();
    }
}