using System;

namespace Rooms.API.Entities
{
    public class Place
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public int Index { get; set; }

        public Guid? OccupiedByUserId { get; set; }

        public DateTime? MovedInAt { get; set; }

        public DateTime? MovedOutAt { get; set; }

        public Room Room { get; set; } = null!;
    }
}