import {
    MarkNotificationsAsReadResponse,
    MyNotificationsResponse,
    NotificationChangesResponse,
    NotificationPreferencesResponse,
    RegisterNotificationReceiptRequest,
    UpdateNotificationPreferencesRequest,
} from "../types/notification";
import api from "../utils/axios-client";

export const notificationService = {
  async getMyNotifications(limit = 5) {
    return api.get<MyNotificationsResponse>("/notifications/me", { limit });
  },

  async markAsRead(ids: string[]) {
    return api.patch<MarkNotificationsAsReadResponse>("/notifications/me/read", { ids });
  },

  async registerReceipt(payload: RegisterNotificationReceiptRequest) {
    return api.post<void>("/notifications/me/receipt", payload);
  },

  async getNotificationChanges(afterCreatedAt?: string, limit = 50) {
    return api.get<NotificationChangesResponse>("/notifications/me/changes", {
      afterCreatedAt,
      limit,
    });
  },

  /* GET /api/notification-settings/{userId} */
  async getPreferences(userId: string) {
    return api.get<NotificationPreferencesResponse>(
      `/notifications/settings/${userId}`,
    );
  },

  /* PATCH /api/notification-settings/me */
  async updatePreferences(payload: UpdateNotificationPreferencesRequest) {
    return api.patch<void>("/notifications/settings/me", payload);
  },
};
