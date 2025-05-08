namespace Rooms.API.Contracts.Block
{
    public sealed class BlockDetailsResponse
    {
        public Guid Id { get; set; }

        public Guid FloorId { get; set; }

        public string Label { get; set; } = string.Empty;

        public string GenderRule { get; set; } = string.Empty;

        public string FloorNumber { get; set; } = string.Empty;

        public string BuildingName { get; set; } = string.Empty;
    }
}