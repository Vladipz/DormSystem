namespace Rooms.API.Entities
{
    public class Floor
    {
        public Guid Id { get; set; }

        public Guid BuildingId { get; set; }

        public int Number { get; set; }

        public int BlocksCount { get; set; }

        public Building Building { get; init; } = null!;

        public ICollection<Block> Blocks { get; init; } = new List<Block>();
    }
}