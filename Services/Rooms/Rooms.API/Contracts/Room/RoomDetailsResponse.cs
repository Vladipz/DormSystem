using System.Collections.ObjectModel;

using Rooms.API.Contracts.Block;
using Rooms.API.Contracts.Building;
using Rooms.API.Entities;

namespace Rooms.API.Contracts.Room
{
    public sealed class RoomDetailsResponse
    {
        public Guid Id { get; init; }

        public BlockInfo? Block { get; init; }

        public int Floor { get; init; }

        public BuildingInfo? Building { get; init; }

        public string Label { get; init; } = string.Empty;

        public int Capacity { get; init; }

        public RoomStatus Status { get; init; }

        public RoomType RoomType { get; init; }

        public string? Purpose { get; init; }

        public Collection<string> Amenities { get; init; } =
            [];
    }
}