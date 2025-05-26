import {
    NotificationPreferencesResponse,
    UpdateNotificationPreferencesRequest,
} from "../types/notification";
import api from "../utils/axios-client";

export const notificationService = {
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
