using System.Diagnostics.Metrics;

namespace NotificationCore.API.Observability
{
    public static class NotificationMetrics
    {
        public const string MeterName = "DormSystem.Notifications";

        private static readonly Meter Meter = new(MeterName);

        public static readonly Histogram<double> DeliveryLatencyMs =
            Meter.CreateHistogram<double>(
                "notifications.delivery_latency_ms",
                unit: "ms",
                description: "Latency from notification creation to client receipt acknowledgement");
    }
}
