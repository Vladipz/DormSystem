namespace Rooms.API.Contracts.Place
{
    public sealed class PlacesResponse
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public int Index { get; set; }

        public bool IsOccupied { get; set; }

        public DateTime? MovedInAt { get; set; }

        public string RoomLabel { get; set; } = string.Empty;
    }
} 