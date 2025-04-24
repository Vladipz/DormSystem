using System;

namespace Rooms.API.Entities
{
    public class Place
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets reference to the room this place belongs to.
        /// Only regular dormitory rooms have places.
        /// </summary>
        public Guid RoomId { get; set; }

        public int Index { get; set; }

        public Guid? OccupiedByUserId { get; set; }

        public DateTime? MovedInAt { get; set; }

        public DateTime? MovedOutAt { get; set; }

        /// <summary>
        /// Gets or sets navigation property for the room this place belongs to.
        /// </summary>
        public Room Room { get; set; } = null!;
    }
}