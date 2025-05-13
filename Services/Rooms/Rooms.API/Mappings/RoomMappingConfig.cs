using Mapster;

using Rooms.API.Contracts.Block;
using Rooms.API.Contracts.Building;
using Rooms.API.Contracts.Room;
using Rooms.API.Entities;

namespace Rooms.API.Mappings
{
    public class RoomMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            config.NewConfig<Room, RoomDetailsResponse>()
                .Map(
                        dest => dest.Block,
                        src => src.Block == null
                        ? null
                        : new BlockInfo
                        {
                            Id = src.Block.Id,
                            Label = src.Block.Label,
                        })

                // Map Floor number based on the available data
                .Map(dest => dest.Floor, src => GetFloorNumber(src))

                // Map Building information based on available data
                .Map(dest => dest.Building, src => GetBuildingInfo(src));
        }

        private static int GetFloorNumber(Room src)
        {
            if (src.Floor != null)
            {
                return src.Floor.Number;
            }

            if (src.Block?.Floor != null)
            {
                return src.Block.Floor.Number;
            }

            return 0;
        }

        private static BuildingInfo GetBuildingInfo(Room src)
        {
            if (src.Building != null)
            {
                return new BuildingInfo
                {
                    Id = src.Building.Id,
                    Label = src.Building.Name,
                };
            }

            if (src.Floor?.Building != null)
            {
                return new BuildingInfo
                {
                    Id = src.Floor.Building.Id,
                    Label = src.Floor.Building.Name,
                };
            }

            if (src.Block?.Floor?.Building != null)
            {
                return new BuildingInfo
                {
                    Id = src.Block.Floor.Building.Id,
                    Label = src.Block.Floor.Building.Name,
                };
            }

            return null;
        }
    }
}