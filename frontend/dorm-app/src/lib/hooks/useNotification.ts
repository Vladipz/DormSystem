import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notificationService } from "../services/notificationService";
import { MyNotificationsResponse, UpdateNotificationPreferencesRequest } from "../types/notification";

/* Cache key helper */
const prefKey = (userId: string) => ["notificationPreferences", userId];
const myNotificationsKey = (userId: string, limit: number) => ["myNotifications", userId, limit];

export function useMyNotifications(userId: string | undefined, limit = 5) {
  return useQuery({
    queryKey: myNotificationsKey(userId ?? "", limit),
    queryFn: () => notificationService.getMyNotifications(limit),
    enabled: !!userId,
  });
}

export function useMarkNotificationsAsRead(userId: string | undefined, limit = 5) {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (ids: string[]) => notificationService.markAsRead(ids),
    onSuccess: (result) => {
      if (!userId || result.readIds.length === 0) {
        return;
      }

      qc.setQueryData<MyNotificationsResponse>(myNotificationsKey(userId, limit), (oldData) => {
        if (!oldData) {
          return oldData;
        }

        const readIdSet = new Set(result.readIds);
        const notifications = oldData.notifications.map((notification) =>
          readIdSet.has(notification.id)
            ? { ...notification, isRead: true }
            : notification,
        );

        return {
          ...oldData,
          unreadCount: Math.max(0, oldData.unreadCount - result.markedCount),
          notifications,
        };
      });

      qc.invalidateQueries({ queryKey: ["myNotifications", userId] });
    },
  });
}

/* 1. read hook */
export function useNotificationPreferences(userId: string | undefined) {
  return useQuery({
    queryKey: prefKey(userId ?? ""),
    queryFn: () => {
      if (!userId) throw new Error("userId required");
      return notificationService.getPreferences(userId);
    },
    enabled: !!userId, // don't run until we have userId
  });
}

/* 2. update hook (optimistic update included) */
export function useUpdateNotificationPreferences(userId: string) {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (payload: UpdateNotificationPreferencesRequest) =>
      notificationService.updatePreferences(payload),
    onSuccess: () => {
      // refetch so UI shows fresh data
      qc.invalidateQueries({ queryKey: prefKey(userId) });
    },
  });
}
