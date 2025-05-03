using Mapster;

using Rooms.API.Contracts.Maintenance;
using Rooms.API.Contracts.Room;
using Rooms.API.Entities;

using Shared.UserServiceClient;

namespace Rooms.API.Mappings
{
    public class MaintenanceTicketMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MaintenanceTicket, MaintenanceTicketResponse>()
                .Map(dest => dest.Room, src => new ShortRoomResponse
                {
                    Id = src.Room.Id,
                    Label = src.Room.Label,
                })

            // Мапимо ReporterById → Reporter.Id
            .Map(
                dest => dest.Reporter,
                src => new UserDto
                {
                    Id = src.ReporterById,
                })

            // Мапимо AssignedToId → AssignedTo.Id (якщо є)
            .Map(
                dest => dest.AssignedTo,
                src => src.AssignedToId.HasValue
                    ? new UserDto { Id = src.AssignedToId.Value }
                    : null);
        }
    }
}