import { Badge, Button, Skeleton } from "@/components/ui";
import { useAuth } from "@/lib/hooks/useAuth";
import {
  useMarkNotificationsAsRead,
  useMyNotifications,
} from "@/lib/hooks/useNotification";
import { createFileRoute } from "@tanstack/react-router";
import { formatDistanceToNow } from "date-fns";
import { Bell, CheckCheck } from "lucide-react";

export const Route = createFileRoute("/_mainLayout/notifications/")({
  component: NotificationsPage,
});

function NotificationsPage() {
  const { user, isAuthenticated } = useAuth();
  const { data: notificationData, isLoading } = useMyNotifications(
    user?.id,
    50,
  );
  const markNotificationsAsRead = useMarkNotificationsAsRead(user?.id, 50);

  const notifications = notificationData?.notifications ?? [];
  const unreadCount = notificationData?.unreadCount ?? 0;
  const unreadNotificationIds = notifications
    .filter((notification) => !notification.isRead)
    .map((notification) => notification.id);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <Badge variant="secondary" className="px-3 py-1 text-xs">
          {unreadCount} unread
        </Badge>
        <Button
          variant="outline"
          disabled={
            unreadNotificationIds.length === 0 ||
            markNotificationsAsRead.isPending
          }
          onClick={() => markNotificationsAsRead.mutate(unreadNotificationIds)}
        >
          <CheckCheck className="mr-2 h-4 w-4" />
          Read all
        </Button>
      </div>
      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      ) : isAuthenticated ? (
        notifications.length > 0 ? (
          <div className="space-y-3">
            {notifications.map((notification) => (
              <div
                key={notification.id}
                className={`rounded-xl border p-4 transition-colors ${notification.isRead ? "opacity-80" : "bg-primary/5 border-primary/20"}`}
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="space-y-1">
                    <div className="flex items-center gap-2">
                      <h3 className="text-sm font-medium">
                        {notification.title}
                      </h3>
                      {!notification.isRead && (
                        <span className="h-2.5 w-2.5 rounded-full bg-blue-500" />
                      )}
                    </div>
                    <p className="text-muted-foreground text-sm">
                      {notification.message}
                    </p>
                  </div>
                  <Badge variant="outline" className="shrink-0 text-[10px]">
                    {notification.type}
                  </Badge>
                </div>

                <div className="mt-4 flex items-center justify-between gap-3">
                  <p className="text-muted-foreground text-xs">
                    {formatDistanceToNow(new Date(notification.createdAt), {
                      addSuffix: true,
                    })}
                  </p>

                  {!notification.isRead && (
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={markNotificationsAsRead.isPending}
                      onClick={() =>
                        markNotificationsAsRead.mutate([notification.id])
                      }
                    >
                      Mark read
                    </Button>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-muted-foreground py-16 text-center">
            <Bell className="mx-auto mb-3 h-10 w-10 opacity-40" />
            <p className="text-base font-medium">No messages yet</p>
            <p className="mt-1 text-sm">
              New in-app notifications will appear here.
            </p>
          </div>
        )
      ) : (
        <div className="text-muted-foreground py-16 text-center">
          <Bell className="mx-auto mb-3 h-10 w-10 opacity-40" />
          <p className="text-base font-medium">Sign in to see your messages</p>
        </div>
      )}
    </div>
  );
}
