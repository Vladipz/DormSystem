// Updated SettingsTab component
import { AccountSettings } from "./AccountSettings";
import { NotificationSettings } from "./NotificationSettings";

interface SettingsTabProps {
  userId: string;
}

export function SettingsTab({ userId }: SettingsTabProps) {
  return (
    <div className="space-y-4">
      <NotificationSettings userId={userId} />
      <AccountSettings />
    </div>
  );
}