namespace NotificationCore.API.Entities
{
    /// <summary>
    /// Specifies the type of notification being sent.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>Notifications about events (e.g., new or updated events).</summary>
        Events,
        /// <summary>Notifications related to room bookings and reservations.</summary>
        RoomBookings,
        /// <summary>Reminders for laundry schedules or pickups.</summary>
        LaundryReminders,
        /// <summary>Updates from the marketplace (offers, listings).</summary>
        MarketplaceUpdates,
        /// <summary>Notifications about purchase requests.</summary>
        PurchaseRequests,
        /// <summary>Results from inspections (pass/fail, notes).</summary>
        InspectionResults,
    }

    /// <summary>
    /// Enumerates channels through which notifications can be delivered.
    /// </summary>
    public enum NotificationChannel
    {
        /// <summary>Delivery via email.</summary>
        Email,
        /// <summary>Delivery via Telegram.</summary>
        Telegram,
        /// <summary>In-application (in-app) delivery.</summary>
        InApp,
    }
}
