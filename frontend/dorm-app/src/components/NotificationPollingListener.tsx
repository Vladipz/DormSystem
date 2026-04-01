import { notificationDeliveryMode } from "@/lib/config/notificationDelivery";
import { useAuth } from "@/lib/hooks/useAuth";
import { notificationService } from "@/lib/services/notificationService";
import type { MyNotificationsResponse } from "@/lib/types/notification";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";
import { toast } from "sonner";

const POLL_INTERVAL_MS = 5000;

export function NotificationPollingListener() {
  const { isAuthenticated, user } = useAuth();
  const queryClient = useQueryClient();
  const lastSeenCreatedAtRef = useRef<string | null>(null);

  useEffect(() => {
    if (
      notificationDeliveryMode !== "polling_5s" ||
      !isAuthenticated ||
      !user?.id
    ) {
      return;
    }

    let cancelled = false;

    const syncNotification = async () => {
      const response = await notificationService.getNotificationChanges(
        lastSeenCreatedAtRef.current ?? undefined,
        50,
      );

      if (cancelled || response.notifications.length === 0) {
        return;
      }

      const cachedNotificationData =
        queryClient.getQueriesData<MyNotificationsResponse>({
          queryKey: ["myNotifications", user.id],
        });
      const existingIds = new Set(
        cachedNotificationData.flatMap(
          ([, data]) =>
            data?.notifications.map((notification) => notification.id) ?? [],
        ),
      );
      const freshNotifications = response.notifications.filter(
        (notification) => !existingIds.has(notification.id),
      );

      if (freshNotifications.length === 0) {
        const newestNotification =
          response.notifications[response.notifications.length - 1];
        if (newestNotification) {
          lastSeenCreatedAtRef.current = newestNotification.createdAt;
        }

        return;
      }

      queryClient.setQueriesData(
        { queryKey: ["myNotifications", user.id] },
        (oldData: MyNotificationsResponse | undefined) => {
          if (!oldData) {
            return oldData;
          }

          return {
            ...oldData,
            unreadCount:
              oldData.unreadCount +
              freshNotifications.filter((notification) => !notification.isRead)
                .length,
            notifications: [
              ...freshNotifications.reverse(),
              ...oldData.notifications,
            ].slice(0, oldData.notifications.length),
          };
        },
      );

      for (const notification of freshNotifications) {
        toast(notification.title, {
          description: notification.message,
        });

        void notificationService.registerReceipt({
          notificationId: notification.id,
          mode: "polling_5s",
          receivedAtUtc: new Date().toISOString(),
        });
      }

      const newestNotification =
        response.notifications[response.notifications.length - 1];
      if (newestNotification) {
        lastSeenCreatedAtRef.current = newestNotification.createdAt;
      }

      void queryClient.invalidateQueries({
        queryKey: ["myNotifications", user.id],
      });
    };

    const initializeMarker = () => {
      const cachedNotificationData =
        queryClient.getQueriesData<MyNotificationsResponse>({
          queryKey: ["myNotifications", user.id],
        });

      for (const [, data] of cachedNotificationData) {
        const newestCreatedAt = data?.notifications[0]?.createdAt;
        if (newestCreatedAt) {
          lastSeenCreatedAtRef.current = newestCreatedAt;
          break;
        }
      }

    };

    initializeMarker();
    void syncNotification();
    const intervalId = window.setInterval(() => {
      void syncNotification();
    }, POLL_INTERVAL_MS);

    return () => {
      cancelled = true;
      window.clearInterval(intervalId);
    };
  }, [isAuthenticated, queryClient, user?.id]);

  return null;
}
