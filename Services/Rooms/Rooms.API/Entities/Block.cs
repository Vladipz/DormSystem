using System;
using System.Collections.Generic;

namespace Rooms.API.Entities
{
    public class Block
    {
        public Guid Id { get; set; }

        public Guid FloorId { get; set; }

        public string Label { get; set; } = string.Empty;

        public string GenderRule { get; set; } = string.Empty;

        public Floor Floor { get; set; } = null!;

        public ICollection<Room> Rooms { get; init; } = new List<Room>();
    }
}