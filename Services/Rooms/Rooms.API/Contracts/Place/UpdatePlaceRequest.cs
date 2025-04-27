namespace Rooms.API.Contracts.Place
{
    public sealed class UpdatePlaceRequest
    {
        public Guid Id { get; set; }
        
        public int Index { get; set; }

        public Guid? OccupiedByUserId { get; set; }

        public DateTime? MovedInAt { get; set; }

        public DateTime? MovedOutAt { get; set; }
    }
} 