namespace Rooms.API.Contracts.Place
{
    public sealed class PlaceDetailsResponse
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public int Index { get; set; }

        public Guid? OccupiedByUserId { get; set; }

        public DateTime? MovedInAt { get; set; }

        public DateTime? MovedOutAt { get; set; }

        public string RoomLabel { get; set; } = string.Empty;
    }
} 