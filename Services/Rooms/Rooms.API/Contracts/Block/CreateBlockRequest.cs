namespace Rooms.API.Contracts.Block
{
    public sealed class CreateBlockRequest
    {
        public Guid FloorId { get; set; }

        public string Label { get; set; } = string.Empty;

        public string GenderRule { get; set; } = string.Empty;
    }
}