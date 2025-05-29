namespace Rooms.API.Contracts.Place
{
    public sealed class UserAddressResponse
    {
        public Guid? PlaceId { get; init; }

        public Guid? RoomId { get; init; }

        public string? RoomLabel { get; init; }

        public Guid? FloorId { get; init; }

        public string? FloorLabel { get; init; }

        public Guid? BuildingId { get; init; }

        public string? BuildingName { get; init; }

        public string? BuildingAddress { get; init; }

        public bool IsOccupied { get; init; }

        public DateTime? MovedInAt { get; init; }
    }
}