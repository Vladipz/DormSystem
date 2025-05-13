namespace Shared.Data
{
    public sealed class EventCreated
    {
        public Guid EventId { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        public string CustomLocation { get; set; } = string.Empty;
        
        public Guid? BuildingId { get; set; }
        
        public Guid? RoomId { get; set; }
        
        public Guid OwnerId { get; set; }
        
        public bool IsPublic { get; set; }
    }
} 