export enum NotificationType {
  Events = "Events",
  RoomBookings = "RoomBookings",
  LaundryReminders = "LaundryReminders",
  MarketplaceUpdates = "MarketplaceUpdates",
  PurchaseRequests = "PurchaseRequests",
  InspectionResults = "InspectionResults",
}

export enum NotificationChannel {
  Email = "Email",
  Telegram = "Telegram",
  WebPush = "WebPush",
}

/* Single setting */
export interface NotificationTypeSetting {
  type: NotificationType;
  enabled: boolean;
}

export interface NotificationChannelSetting {
  channel: NotificationChannel;
  enabled: boolean;
  externalId?: string | null;
}

/* ----- API contracts ----- */
export interface NotificationPreferencesResponse {
  userId: string;
  settings: NotificationTypeSetting[];
  channels: NotificationChannelSetting[];
}

export interface UpdateNotificationPreferencesRequest {
  types: NotificationTypeSetting[];
  channels: NotificationChannelSetting[];
}
