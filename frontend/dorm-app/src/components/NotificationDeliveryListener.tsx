import { notificationDeliveryMode } from "@/lib/config/notificationDelivery";
import { NotificationPollingListener } from "./NotificationPollingListener";
import { NotificationRealtimeListener } from "./NotificationRealtimeListener";

export function NotificationDeliveryListener() {
  if (notificationDeliveryMode === "polling_5s") {
    return <NotificationPollingListener />;
  }

  return <NotificationRealtimeListener />;
}
