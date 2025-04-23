using System;

namespace Rooms.API.Entities
{
    public class Block
    {
        public Guid Id { get; set; }

        public Guid FloorId { get; set; }

        public string Label { get; set; } = string.Empty;

        public string GenderRule { get; set; } = string.Empty;
    }
}