using Rooms.API.Entities;

namespace Rooms.API.Contracts.Room
{
    public sealed class UpdateRoomRequest
    {
        public Guid Id { get; set; }

        public Guid? BlockId { get; set; }

        public string Label { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public RoomStatus Status { get; set; }

        public RoomType RoomType { get; set; }

        public string? Purpose { get; set; }

        public List<string> Amenities { get; set; } = new();

        /// <summary>
        /// Gets or sets the IDs of photos to associate with the room.
        /// </summary>
        public List<string> PhotoIds { get; set; } = new();
    }
}