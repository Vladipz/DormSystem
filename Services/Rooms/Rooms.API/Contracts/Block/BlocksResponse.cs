namespace Rooms.API.Contracts.Block
{
    public sealed class BlocksResponse
    {
        public Guid Id { get; set; }

        public Guid FloorId { get; set; }

        public string Label { get; set; } = string.Empty;

        public string GenderRule { get; set; } = string.Empty;

        public int RoomsCount { get; set; }
    }
} 