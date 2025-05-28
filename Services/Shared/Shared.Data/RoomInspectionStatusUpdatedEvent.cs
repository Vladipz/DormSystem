namespace Shared.Data;

public sealed class RoomInspectionStatusUpdatedEvent
{
    public Guid InspectionId { get; set; }
    
    public Guid RoomInspectionId { get; set; }
    
    public Guid RoomId { get; set; }
    
    public string InspectionName { get; set; } = string.Empty;
    
    public string InspectionType { get; set; } = string.Empty;
    
    public string NewStatus { get; set; } = string.Empty;
    
    public string? Comment { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public string RoomNumber { get; set; } = string.Empty;
    
    public string Building { get; set; } = string.Empty;
    
    public string Floor { get; set; } = string.Empty;
} 