namespace Rooms.API.Contracts.Floor
{
    public sealed class CreateFloorRequest
    {
        public Guid BuildingId { get; set; }

        public int Number { get; set; }
    }
} 