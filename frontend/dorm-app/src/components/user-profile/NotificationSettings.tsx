import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  useNotificationPreferences,
  useUpdateNotificationPreferences,
} from "@/lib/hooks/useNotification";
import {
  NotificationChannel,
  NotificationChannelSetting,
  NotificationType,
  NotificationTypeSetting,
} from "@/lib/types/notification";
import {
  Bell,
  Calendar,
  Mail,
  MessageCircle,
  ShieldCheck,
  ShoppingCart,
  WashingMachine,
} from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

// ===================================================================
// NOTIFICATION IMPLEMENTATION STATUS CONFIGURATION
// ===================================================================
// This section tracks which notification types and channels are implemented
// and ready for production use. Update these lists as features are completed.

/**
 * Notification Types Implementation Status
 * - IMPLEMENTED: Fully functional and ready for use
 * - SOON: Planned for implementation but not yet available
 */
const NOTIFICATION_TYPES_CONFIG = {
  IMPLEMENTED: [
    NotificationType.Events,
    NotificationType.InspectionResults
  ] as NotificationType[],
  
  SOON: [
    NotificationType.RoomBookings,
    NotificationType.LaundryReminders,
    NotificationType.MarketplaceUpdates,
    NotificationType.PurchaseRequests,
  ] as NotificationType[],
} as const;

/**
 * Notification Channels Implementation Status
 * - IMPLEMENTED: Fully functional and ready for use
 * - SOON: Planned for implementation but not yet available
 */
const NOTIFICATION_CHANNELS_CONFIG = {
  IMPLEMENTED: [
    NotificationChannel.Telegram,
  ] as NotificationChannel[],
  
  SOON: [
    NotificationChannel.Email,
    NotificationChannel.WebPush,
  ] as NotificationChannel[],
} as const;

/**
 * Helper function to check if a notification type is implemented
 */
const isNotificationTypeImplemented = (type: NotificationType): boolean => {
  return NOTIFICATION_TYPES_CONFIG.IMPLEMENTED.includes(type);
};

/**
 * Helper function to check if a notification channel is implemented
 */
const isNotificationChannelImplemented = (channel: NotificationChannel): boolean => {
  return NOTIFICATION_CHANNELS_CONFIG.IMPLEMENTED.includes(channel);
};

/**
 * Get all notification types (implemented + soon)
 */
const getAllNotificationTypes = (): NotificationType[] => {
  return [...NOTIFICATION_TYPES_CONFIG.IMPLEMENTED, ...NOTIFICATION_TYPES_CONFIG.SOON];
};

/**
 * Get all notification channels (implemented + soon)
 */
const getAllNotificationChannels = (): NotificationChannel[] => {
  return [...NOTIFICATION_CHANNELS_CONFIG.IMPLEMENTED, ...NOTIFICATION_CHANNELS_CONFIG.SOON];
};

// ===================================================================
// END CONFIGURATION SECTION
// ===================================================================

/*
 * IMPLEMENTATION STATUS SUMMARY
 * =============================
 * 
 * ✅ IMPLEMENTED FEATURES:
 * Notification Types:
 *   - Events: Fully functional event notifications
 * 
 * Notification Channels:
 *   - Telegram: Bot integration with user authentication
 * 
 * 🔄 COMING SOON (Priority order):
 * Notification Types:
 *   1. RoomBookings - Room reservation confirmations and reminders
 *   2. LaundryReminders - Washing machine availability and completion alerts
 *   3. InspectionResults - Dormitory inspection results and follow-ups
 *   4. MarketplaceUpdates - New items and price changes in marketplace
 *   5. PurchaseRequests - Purchase request status updates
 * 
 * Notification Channels:
 *   1. Email - SMTP integration for email notifications
 *   2. WebPush - Browser push notifications (service worker required)
 * 
 * TODO FOR DEVELOPERS:
 * - Move items from SOON to IMPLEMENTED as features are completed
 * - Update the configuration arrays above when new features are added
 * - Consider adding priority levels or estimated implementation dates
 */

interface NotificationSettingsProps {
  userId: string;
}

// Helper function to get icon for notification type
const getNotificationTypeIcon = (type: NotificationType) => {
  switch (type) {
    case NotificationType.Events:
      return <Calendar className="h-4 w-4" />;
    case NotificationType.RoomBookings:
      return <Calendar className="h-4 w-4" />;
    case NotificationType.LaundryReminders:
      return <WashingMachine className="h-4 w-4" />;
    case NotificationType.MarketplaceUpdates:
      return <ShoppingCart className="h-4 w-4" />;
    case NotificationType.PurchaseRequests:
      return <ShoppingCart className="h-4 w-4" />;
    case NotificationType.InspectionResults:
      return <ShieldCheck className="h-4 w-4" />;
    default:
      return <Bell className="h-4 w-4" />;
  }
};

// Helper function to get display name for notification type
const getNotificationTypeLabel = (type: NotificationType): string => {
  switch (type) {
    case NotificationType.Events:
      return "Events";
    case NotificationType.RoomBookings:
      return "Room Bookings";
    case NotificationType.LaundryReminders:
      return "Laundry Reminders";
    case NotificationType.MarketplaceUpdates:
      return "Marketplace Updates";
    case NotificationType.PurchaseRequests:
      return "Purchase Requests";
    case NotificationType.InspectionResults:
      return "Inspection Results";
    default:
      return type;
  }
};

// Helper function to get icon for notification channel
const getNotificationChannelIcon = (channel: NotificationChannel) => {
  switch (channel) {
    case NotificationChannel.Email:
      return <Mail className="h-4 w-4" />;
    case NotificationChannel.Telegram:
      return <MessageCircle className="h-4 w-4" />;
    case NotificationChannel.WebPush:
      return <Bell className="h-4 w-4" />;
    default:
      return <Bell className="h-4 w-4" />;
  }
};

export function NotificationSettings({ userId }: NotificationSettingsProps) {
  const {
    data: preferences,
    isLoading,
    error,
  } = useNotificationPreferences(userId);

  const updateMutation = useUpdateNotificationPreferences(userId);

  // Local state for tracking changes
  const [localTypeSettings, setLocalTypeSettings] = useState<
    NotificationTypeSetting[]
  >([]);
  const [localChannelSettings, setLocalChannelSettings] = useState<
    NotificationChannelSetting[]
  >([]);
  const [hasChanges, setHasChanges] = useState(false);
  
  // Track original values to detect changes
  const [originalTypeSettings, setOriginalTypeSettings] = useState<
    NotificationTypeSetting[]
  >([]);
  const [originalChannelSettings, setOriginalChannelSettings] = useState<
    NotificationChannelSetting[]
  >([]);
  // Initialize local state when preferences are loaded
  useEffect(() => {
    if (preferences) {
      // Create default type settings if not present
      const defaultTypeSettings: NotificationTypeSetting[] = getAllNotificationTypes().map((type) => {
        const existingSetting = preferences.settings.find(s => s.type === type);
        return existingSetting || { type, enabled: false };
      });

      // Create default channel settings if not present
      const defaultChannelSettings: NotificationChannelSetting[] = getAllNotificationChannels().map((channel) => {
        const existingChannel = preferences.channels.find(c => c.channel === channel);
        return existingChannel || { channel, enabled: false, externalId: null };
      });

      setLocalTypeSettings(defaultTypeSettings);
      setLocalChannelSettings(defaultChannelSettings);
      setOriginalTypeSettings(defaultTypeSettings);
      setOriginalChannelSettings(defaultChannelSettings);
      setHasChanges(false);
    }
  }, [preferences]);

  // Handle mutation status with toast notifications
  useEffect(() => {
    if (updateMutation.isSuccess) {
      toast.success("Notification preferences saved successfully!");
    } else if (updateMutation.isError) {
      toast.error("Failed to save notification preferences. Please try again.");
    }
  }, [updateMutation.isSuccess, updateMutation.isError]);

  // Convert local data to maps for easier lookup
  const typeSettingsMap = useMemo(() => {
    return new Map(
      localTypeSettings.map((setting) => [setting.type, setting.enabled]),
    );
  }, [localTypeSettings]);

  const channelSettingsMap = useMemo(() => {
    return new Map(
      localChannelSettings.map((channel) => [channel.channel, channel]),
    );
  }, [localChannelSettings]);

  const handleTypeToggle = (type: NotificationType) => {
    setLocalTypeSettings((prevSettings) => {
      const updatedSettings = prevSettings.map((setting) =>
        setting.type === type
          ? { ...setting, enabled: !setting.enabled }
          : setting,
      );
      setHasChanges(true);
      return updatedSettings;
    });
  };

  const handleChannelToggle = (channel: NotificationChannel) => {
    setLocalChannelSettings((prevChannels) => {
      const updatedChannels = prevChannels.map((ch) =>
        ch.channel === channel ? { ...ch, enabled: !ch.enabled } : ch,
      );
      setHasChanges(true);
      return updatedChannels;
    });
  };
  const handleSave = () => {
    toast.loading("Saving notification preferences...");
    
    // Find only the changed type settings
    const changedTypeSettings = localTypeSettings.filter((localSetting) => {
      const originalSetting = originalTypeSettings.find(orig => orig.type === localSetting.type);
      return !originalSetting || originalSetting.enabled !== localSetting.enabled;
    });

    // Find only the changed channel settings
    const changedChannelSettings = localChannelSettings.filter((localChannel) => {
      const originalChannel = originalChannelSettings.find(orig => orig.channel === localChannel.channel);
      return !originalChannel || 
             originalChannel.enabled !== localChannel.enabled ||
             originalChannel.externalId !== localChannel.externalId;
    });

    // Create payload with only changed data, but ensure arrays are always present (even if empty)
    const payload = {
      types: changedTypeSettings,
      channels: changedChannelSettings,
    };

    updateMutation.mutate(
      payload,
      {
        onSuccess: () => {
          // Update original values to reflect the new state
          setOriginalTypeSettings([...localTypeSettings]);
          setOriginalChannelSettings([...localChannelSettings]);
          setHasChanges(false);
          toast.dismiss(); // Dismiss the loading toast
        },
        onError: () => {
          toast.dismiss(); // Dismiss the loading toast
        },
      },
    );
  };

  const handleReset = () => {
    if (preferences) {
      // Create default type settings if not present (same logic as useEffect)
      const defaultTypeSettings: NotificationTypeSetting[] = getAllNotificationTypes().map((type) => {
        const existingSetting = preferences.settings.find(s => s.type === type);
        return existingSetting || { type, enabled: false };
      });

      // Create default channel settings if not present (same logic as useEffect)
      const defaultChannelSettings: NotificationChannelSetting[] = getAllNotificationChannels().map((channel) => {
        const existingChannel = preferences.channels.find(c => c.channel === channel);
        return existingChannel || { channel, enabled: false, externalId: null };
      });

      setLocalTypeSettings(defaultTypeSettings);
      setLocalChannelSettings(defaultChannelSettings);
      setOriginalTypeSettings(defaultTypeSettings);
      setOriginalChannelSettings(defaultChannelSettings);
      setHasChanges(false);
    }
  };

  if (isLoading) {
    return (
      <Card className="mb-4">
        <CardContent className="pt-6">
          <div className="text-center">Loading notification settings...</div>
        </CardContent>
      </Card>
    );
  }
  if (error) {
    toast.error("Failed to load notification settings. Please try again.");
    return (
      <Card className="mb-4">
        <CardContent className="pt-6">
          <div className="text-center text-destructive">
            Failed to load notification settings. Please try again.
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="mb-4">
      <CardHeader>
        <CardTitle>Notification Settings</CardTitle>
        <CardDescription>Manage your notification preferences</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="space-y-6">
          {/* Notification Types */}
          <div>
            <h3 className="mb-3 text-sm font-medium">Notification Types</h3>
            <div className="space-y-3">
              {getAllNotificationTypes().map((type) => (
                <div key={type} className="flex items-center justify-between">
                  <Label
                    htmlFor={`${type}-notifications`}
                    className={`flex items-center gap-2 ${
                      isNotificationTypeImplemented(type) ? "cursor-pointer" : "cursor-not-allowed"
                    }`}
                  >
                    {getNotificationTypeIcon(type)}
                    {getNotificationTypeLabel(type)}
                    {!isNotificationTypeImplemented(type) && (
                      <span className="text-muted-foreground text-xs">
                        (Soon...)
                      </span>
                    )}
                  </Label>
                  <Switch
                    id={`${type}-notifications`}
                    checked={typeSettingsMap.get(type) ?? false}
                    onCheckedChange={() => handleTypeToggle(type)}
                    disabled={updateMutation.isPending || !isNotificationTypeImplemented(type)}
                  />
                </div>
              ))}
            </div>
          </div>

          {/* Notification Channels */}
          <div>
            <h3 className="mb-3 text-sm font-medium">Notification Channels</h3>
            <div className="space-y-3">
              {getAllNotificationChannels().map((channel) => {
                const channelSetting = channelSettingsMap.get(channel);
                return (
                  <div
                    key={channel}
                    className="flex items-center justify-between"
                  >
                    <Label
                      htmlFor={`${channel}-notifications`}
                      className={`flex items-center gap-2 ${
                        isNotificationChannelImplemented(channel) ? "cursor-pointer" : "cursor-not-allowed"
                      }`}
                    >
                      {getNotificationChannelIcon(channel)}
                      {channel}
                      {channel === NotificationChannel.Telegram &&
                        channelSetting?.externalId && (
                          <span className="text-muted-foreground text-xs">
                            (Connected)
                          </span>
                        )}
                      {!isNotificationChannelImplemented(channel) && (
                        <span className="text-muted-foreground text-xs">
                          (Soon...)
                        </span>
                      )}
                    </Label>
                    <Switch
                      id={`${channel}-notifications`}
                      checked={channelSetting?.enabled ?? false}
                      onCheckedChange={() => handleChannelToggle(channel)}
                      disabled={updateMutation.isPending || !isNotificationChannelImplemented(channel)}
                    />
                  </div>
                );
              })}
            </div>
          </div>          {/* Changes indicator */}
          {hasChanges && !updateMutation.isPending && (
            <div className="rounded-md bg-blue-50 border border-blue-200 p-3">
              <div className="text-sm text-blue-700">
                You have unsaved changes. Click "Save Changes" to apply them.
              </div>
            </div>
          )}
        </div>
      </CardContent>
      <CardFooter className="flex justify-end gap-2">
        {hasChanges && (
          <Button
            variant="outline"
            onClick={handleReset}
            disabled={updateMutation.isPending}
          >
            Reset Changes
          </Button>
        )}
        <Button
          onClick={handleSave}
          disabled={updateMutation.isPending || !hasChanges}
        >
          {updateMutation.isPending ? "Saving..." : "Save Changes"}
        </Button>
      </CardFooter>
    </Card>
  );
}
