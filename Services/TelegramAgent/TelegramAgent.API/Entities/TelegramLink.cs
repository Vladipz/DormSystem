namespace TelegramAgent.API.Entities
{
    public class TelegramLink
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public long ChatId { get; set; }
        public DateTime LinkedAt { get; set; }
    }
}