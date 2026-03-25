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
  InApp = "InApp",
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

export interface UserNotification {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  createdAt: string;
  isRead: boolean;
}

export interface MyNotificationsResponse {
  userId: string;
  unreadCount: number;
  notifications: UserNotification[];
}

export interface MarkNotificationsAsReadRequest {
  ids: string[];
}

export interface MarkNotificationsAsReadResponse {
  markedCount: number;
  readIds: string[];
}
