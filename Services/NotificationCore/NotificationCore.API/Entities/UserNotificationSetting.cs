namespace NotificationCore.API.Entities
{
    public class UserNotificationSetting
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public NotificationType NotificationType { get; set; }

        public bool Enabled { get; set; } = true;
    }
}