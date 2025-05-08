namespace Rooms.API.Contracts.Block
{
    public class BlockInfo
    {
        public Guid Id { get; init; }

        public string Label { get; init; } = string.Empty;
    }
}