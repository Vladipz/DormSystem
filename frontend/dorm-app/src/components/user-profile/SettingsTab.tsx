import { AccountSettings } from "./AccountSettings";
import { NotificationSettings } from "./NotificationSettings";

type NotificationSettings = {
  events: boolean;
  bookings: boolean;
  laundry: boolean;
  marketplace: boolean;
  requests: boolean;
  telegram: boolean;
  email: boolean;
};

interface SettingsTabProps {
  notificationSettings: NotificationSettings;
  onNotificationChange: (key: keyof NotificationSettings) => void;
}

export function SettingsTab({
  notificationSettings,
  onNotificationChange,
}: SettingsTabProps) {
  return (
    <div className="space-y-4">
      <NotificationSettings
        settings={notificationSettings}
        onSettingChange={onNotificationChange}
      />
      <AccountSettings />
    </div>
  );
} 