using Rooms.API.Entities;

namespace Rooms.API.Contracts.Room
{
    public sealed class CreateRoomRequest
    {
        public Guid? BlockId { get; set; }

        public Guid? FloorId { get; set; }

        public string Label { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public RoomStatus Status { get; set; }

        public RoomType RoomType { get; set; }

        public string? Purpose { get; set; }

        public ICollection<string> Amenities { get; init; } = new List<string>();
    }
}