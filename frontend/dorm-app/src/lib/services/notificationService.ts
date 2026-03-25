import {
    MarkNotificationsAsReadResponse,
    MyNotificationsResponse,
    NotificationPreferencesResponse,
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
