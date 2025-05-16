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
import { Calendar, ShoppingCart } from "lucide-react";

interface NotificationSettingsProps {
  settings: {
    events: boolean;
    bookings: boolean;
    laundry: boolean;
    marketplace: boolean;
    requests: boolean;
    telegram: boolean;
    email: boolean;
  };
  onSettingChange: (key: keyof NotificationSettings) => void;
}

// Define type for notification settings
type NotificationSettings = {
  events: boolean;
  bookings: boolean;
  laundry: boolean;
  marketplace: boolean;
  requests: boolean;
  telegram: boolean;
  email: boolean;
};

export function NotificationSettings({
  settings,
  onSettingChange,
}: NotificationSettingsProps) {
  return (
    <Card className="mb-4">
      <CardHeader>
        <CardTitle>Notification Settings</CardTitle>
        <CardDescription>Manage your notification preferences</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <div>
            <h3 className="mb-3 text-sm font-medium">Notification Types</h3>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label
                  htmlFor="events-notifications"
                  className="flex items-center gap-2"
                >
                  <Calendar className="h-4 w-4" />
                  Events
                </Label>
                <Switch
                  id="events-notifications"
                  checked={settings.events}
                  onCheckedChange={() => onSettingChange("events")}
                />
              </div>
              <div className="flex items-center justify-between">
                <Label
                  htmlFor="bookings-notifications"
                  className="flex items-center gap-2"
                >
                  <Calendar className="h-4 w-4" />
                  Room Bookings
                </Label>
                <Switch
                  id="bookings-notifications"
                  checked={settings.bookings}
                  onCheckedChange={() => onSettingChange("bookings")}
                />
              </div>
              <div className="flex items-center justify-between">
                <Label
                  htmlFor="laundry-notifications"
                  className="flex items-center gap-2"
                >
                  <Calendar className="h-4 w-4" />
                  Laundry Reminders
                </Label>
                <Switch
                  id="laundry-notifications"
                  checked={settings.laundry}
                  onCheckedChange={() => onSettingChange("laundry")}
                />
              </div>
              <div className="flex items-center justify-between">
                <Label
                  htmlFor="marketplace-notifications"
                  className="flex items-center gap-2"
                >
                  <ShoppingCart className="h-4 w-4" />
                  Marketplace Updates
                </Label>
                <Switch
                  id="marketplace-notifications"
                  checked={settings.marketplace}
                  onCheckedChange={() => onSettingChange("marketplace")}
                />
              </div>
              <div className="flex items-center justify-between">
                <Label
                  htmlFor="requests-notifications"
                  className="flex items-center gap-2"
                >
                  <ShoppingCart className="h-4 w-4" />
                  Purchase Requests
                </Label>
                <Switch
                  id="requests-notifications"
                  checked={settings.requests}
                  onCheckedChange={() => onSettingChange("requests")}
                />
              </div>
            </div>
          </div>
          <div>
            <h3 className="mb-3 text-sm font-medium">Notification Channels</h3>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="telegram-notifications">Telegram</Label>
                <Switch
                  id="telegram-notifications"
                  checked={settings.telegram}
                  onCheckedChange={() => onSettingChange("telegram")}
                />
              </div>
              <div className="flex items-center justify-between">
                <Label htmlFor="email-notifications">Email</Label>
                <Switch
                  id="email-notifications"
                  checked={settings.email}
                  onCheckedChange={() => onSettingChange("email")}
                />
              </div>
            </div>
          </div>
        </div>
      </CardContent>
      <CardFooter>
        <Button className="ml-auto">Save Preferences</Button>
      </CardFooter>
    </Card>
  );
}
