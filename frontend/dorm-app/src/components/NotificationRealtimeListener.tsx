import { notificationDeliveryMode } from "@/lib/config/notificationDelivery";
import { useAuth } from "@/lib/hooks/useAuth";
import { authService } from "@/lib/services/authService";
import { notificationService } from "@/lib/services/notificationService";
import type {
  MyNotificationsResponse,
  UserNotification,
} from "@/lib/types/notification";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";
import { toast } from "sonner";

const VITE_API_GATEWAY_URL =
  import.meta.env.VITE_API_GATEWAY_URL ?? "http://localhost:5095";
const NOTIFICATION_RECEIVED_EVENT = "notificationReceived";

export function NotificationRealtimeListener() {
  const { isAuthenticated, user } = useAuth();
  const queryClient = useQueryClient();
  const connectionRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    if (notificationDeliveryMode !== "websocket") {
      return;
    }

    const stopConnection = async () => {
      if (connectionRef.current) {
        await connectionRef.current.stop();
        connectionRef.current = null;
      }
    };

    if (!isAuthenticated || !user?.id) {
      void stopConnection();
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`${VITE_API_GATEWAY_URL}/api/notifications/hubs/in-app`, {
        withCredentials: false,
        accessTokenFactory: async () => {
          if (authService.isTokenExpired()) {
            await authService.refreshToken();
          }

          return localStorage.getItem("accessToken") ?? "";
        },
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on(
      NOTIFICATION_RECEIVED_EVENT,
      (notification: UserNotification) => {
        toast(notification.title, {
          description: notification.message,
        });

        queryClient.setQueriesData(
          { queryKey: ["myNotifications", user.id] },
          (oldData: MyNotificationsResponse | undefined) => {
            if (!oldData) {
              return oldData;
            }

            if (
              oldData.notifications.some(
                (existing) => existing.id === notification.id,
              )
            ) {
              return oldData;
            }

            return {
              ...oldData,
              unreadCount: oldData.unreadCount + (notification.isRead ? 0 : 1),
              notifications: [notification, ...oldData.notifications].slice(
                0,
                oldData.notifications.length,
              ),
            };
          },
        );

        void queryClient.invalidateQueries({
          queryKey: ["myNotifications", user.id],
        });

        void notificationService.registerReceipt({
          notificationId: notification.id,
          mode: "websocket",
          receivedAtUtc: new Date().toISOString(),
        });
      },
    );

    connection.onreconnected(() =>
      queryClient.invalidateQueries({ queryKey: ["myNotifications", user.id] }),
    );

    void connection.start().catch((error: unknown) => {
      console.error("Failed to start notification SignalR connection", error);
    });

    return () => {
      connection.off(NOTIFICATION_RECEIVED_EVENT);
      void stopConnection();
    };
  }, [isAuthenticated, queryClient, user?.id]);

  return null;
}
