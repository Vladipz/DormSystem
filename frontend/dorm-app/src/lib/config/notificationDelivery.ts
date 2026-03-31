import type { NotificationDeliveryMode } from "@/lib/types/notification";

export const notificationDeliveryMode: NotificationDeliveryMode =
  (import.meta.env.VITE_NOTIFICATIONS_DELIVERY_MODE as NotificationDeliveryMode | undefined) ??
  "websocket";
