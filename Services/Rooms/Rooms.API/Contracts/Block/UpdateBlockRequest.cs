namespace Rooms.API.Contracts.Block
{
    public sealed class UpdateBlockRequest
    {
        public string Label { get; set; } = string.Empty;

        public string GenderRule { get; set; } = string.Empty;
    }
}