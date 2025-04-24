using System;
using System.Collections.Generic;

namespace Rooms.API.Entities
{
    public class Floor
    {
        public Guid Id { get; set; }

        public Guid BuildingId { get; set; }

        public int Number { get; set; }

        public int BlocksCount { get; set; }

        /// <summary>
        /// Gets navigation property for the building this floor belongs to.
        /// </summary>
        public Building Building { get; init; } = null!;

        /// <summary>
        /// Gets navigation property for the blocks on this floor.
        /// </summary>
        public ICollection<Block> Blocks { get; init; } = new List<Block>();
    }
}