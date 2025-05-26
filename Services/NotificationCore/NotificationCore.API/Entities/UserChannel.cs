namespace NotificationCore.API.Entities
{
    public class UserChannel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public NotificationChannel Channel { get; set; }

        public bool Enabled { get; set; } = true;

        public string? ExternalReference { get; set; } // e.g., Telegram chat id
    }
}