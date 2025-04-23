using System;

namespace Rooms.API.Entities
{
    public class Floor
    {
        public Guid Id { get; set; }

        public Guid BuildingId { get; set; }

        public int Number { get; set; }

        public int BlocksCount { get; set; }
    }
}