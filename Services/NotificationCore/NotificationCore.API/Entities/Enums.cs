namespace NotificationCore.API.Entities
{
    public enum NotificationType
    {
        Events,
        RoomBookings,
        LaundryReminders,
        MarketplaceUpdates,
        PurchaseRequests,
        InspectionResults
    }

    public enum NotificationChannel
    {
        Email,
        Telegram,
        WebPush
    }
}