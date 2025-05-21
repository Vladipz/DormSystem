using Inspections.API.Entities;

namespace Inspections.API.Contracts.Inspections
{
    public class InspectionListItemResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public InspectionStatus Status { get; set; }

        public int RoomsCount { get; set; }

        public int PendingRoomsCount { get; set; }

        public int ConfirmedRoomsCount { get; set; }

        public int NotConfirmedRoomsCount { get; set; }

        public int NoAccessRoomsCount { get; set; }
    }
}