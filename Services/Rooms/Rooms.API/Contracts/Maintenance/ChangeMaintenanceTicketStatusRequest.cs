using Rooms.API.Entities;

namespace Rooms.API.Contracts.Maintenance
{
    public sealed class ChangeMaintenanceTicketStatusRequest
    {
        public Guid TicketId { get; set; }

        public MaintenanceStatus NewStatus { get; set; }
    }
}