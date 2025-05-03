using System.Collections.ObjectModel;

using Rooms.API.Entities;

namespace Rooms.API.Contracts.Room
{
    public sealed class RoomDetailsResponse
    {
        public Guid Id { get; init; }

        public Guid? BlockId { get; init; }
        //add a block field to the response

        public string Label { get; init; } = string.Empty;

        public int Capacity { get; init; }

        public RoomStatus Status { get; init; }

        public RoomType RoomType { get; init; }

        public string? Purpose { get; init; }

        public Collection<string> Amenities { get; init; } =
            [];
    }
}