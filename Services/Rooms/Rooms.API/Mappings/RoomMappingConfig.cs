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

                // Flatten Floor number from nested Floor
                .Map(
                    dest => dest.Floor,
                    src => src.Block == null
                    ? 0
                    : src.Block.Floor.Number)

                // Map Building navigation to BuildingInfo DTO
                .Map(
                        dest => dest.Building,
                        src => src.Block == null || src.Block.Floor.Building == null
                        ? null
                        : new BuildingInfo
                        {
                            Id = src.Block.Floor.Building.Id,
                            Label = src.Block.Floor.Building.Name,
                        });
        }
    }
}