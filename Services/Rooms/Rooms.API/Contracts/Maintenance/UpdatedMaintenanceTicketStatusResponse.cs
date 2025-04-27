using Rooms.API.Entities;

namespace Rooms.API.Contracts.Maintenance
{
    public sealed class UpdatedMaintenanceTicketStatusResponse
    {
        public Guid Id { get; set; }

        public MaintenanceStatus NewStatus { get; set; }
    }
}