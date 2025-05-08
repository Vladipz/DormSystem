namespace Rooms.API.Contracts.Place
{
    public sealed class CreatePlaceRequest
    {
        public Guid RoomId { get; set; }

        public int Index { get; set; }

        public Guid? OccupiedByUserId { get; set; }

        public DateTime? MovedInAt { get; set; }
    }
}