namespace Rooms.API.Contracts.Floor
{
    public sealed class FloorsResponse
    {
        public Guid Id { get; set; }

        public Guid BuildingId { get; set; }

        public int Number { get; set; }

        public int BlocksCount { get; set; }

        public string BuildingName { get; set; } = string.Empty;
    }
} 