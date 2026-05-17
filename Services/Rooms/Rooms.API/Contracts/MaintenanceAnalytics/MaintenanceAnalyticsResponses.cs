using Rooms.API.Entities;

using Shared.UserServiceClient;

namespace Rooms.API.Contracts.MaintenanceAnalytics
{
    public sealed class MaintenanceHeatmapResponse
    {
        public Guid BuildingId { get; set; }

        public string BuildingName { get; set; } = string.Empty;

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int MaxTicketsCount { get; set; }

        public List<MaintenanceHeatmapCellResponse> Cells { get; set; } = [];
    }

    public sealed class MaintenanceHeatmapCellResponse
    {
        public Guid FloorId { get; set; }

        public int FloorNumber { get; set; }

        public Guid BlockId { get; set; }

        public string BlockLabel { get; set; } = string.Empty;

        public int TicketsCount { get; set; }

        public int OpenCount { get; set; }

        public int InProgressCount { get; set; }

        public int ResolvedCount { get; set; }

        public decimal ResolvedPercentage { get; set; }

        public MaintenancePriority? MostFrequentPriority { get; set; }
    }

    public sealed class MaintenanceDrilldownResponse
    {
        public Guid BuildingId { get; set; }

        public string BuildingName { get; set; } = string.Empty;

        public Guid FloorId { get; set; }

        public int FloorNumber { get; set; }

        public Guid BlockId { get; set; }

        public string BlockLabel { get; set; } = string.Empty;

        public MaintenanceDrilldownSummaryResponse Summary { get; set; } = new();

        public List<MaintenanceDrilldownTicketResponse> Tickets { get; set; } = [];

        public List<MaintenanceRoomBarResponse> RoomBars { get; set; } = [];

        public List<MaintenanceTimelinePointResponse> Timeline { get; set; } = [];

        public string DiagnosticMessage { get; set; } = string.Empty;
    }

    public sealed class MaintenanceDrilldownSummaryResponse
    {
        public int TicketsCount { get; set; }

        public int OpenCount { get; set; }

        public int InProgressCount { get; set; }

        public int ResolvedCount { get; set; }

        public decimal ResolvedPercentage { get; set; }

        public decimal AverageDaysInWork { get; set; }

        public string? MostLoadedRoomLabel { get; set; }

        public MaintenancePriority? MostFrequentPriority { get; set; }
    }

    public sealed class MaintenanceDrilldownTicketResponse
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public string RoomLabel { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public UserDto? AssignedTo { get; set; }

        public MaintenanceStatus Status { get; set; }

        public MaintenancePriority Priority { get; set; }

        public int DaysInWork { get; set; }
    }

    public sealed class MaintenanceRoomBarResponse
    {
        public Guid RoomId { get; set; }

        public string RoomLabel { get; set; } = string.Empty;

        public int TicketsCount { get; set; }
    }

    public sealed class MaintenanceTimelinePointResponse
    {
        public string Label { get; set; } = string.Empty;

        public DateTime PeriodStart { get; set; }

        public int TicketsCount { get; set; }
    }
}
