namespace Rooms.API.Contracts.Room
{
    public class ShortRoomResponse
    {
        public Guid Id { get; init; }

        public string Label { get; init; } = string.Empty;
    }
}