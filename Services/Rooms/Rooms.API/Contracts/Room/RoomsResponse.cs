using Rooms.API.Entities;

namespace Rooms.API.Contracts.Room
{
    public class RoomsResponse
    {
        public Guid Id { get; init; }

        public Guid? BlockId { get; init; }

        public string Label { get; init; } = string.Empty;

        public int Capacity { get; init; }

        public RoomStatus Status { get; init; }

        public RoomType RoomType { get; init; }
    }
}