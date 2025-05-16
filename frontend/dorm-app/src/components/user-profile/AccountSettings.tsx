import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { LogOut } from "lucide-react";

export function AccountSettings() {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Account Settings</CardTitle>
        <CardDescription>Manage your account settings</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex items-center justify-between">
          <div className="space-y-0.5">
            <h3 className="font-medium">Change Password</h3>
            <p className="text-muted-foreground text-sm">
              Update your account password
            </p>
          </div>
          <Button variant="outline">Change</Button>
        </div>
        <div className="flex items-center justify-between">
          <div className="space-y-0.5">
            <h3 className="font-medium">Language Preferences</h3>
            <p className="text-muted-foreground text-sm">
              Change your language settings
            </p>
          </div>
          <Button variant="outline">Edit</Button>
        </div>
        <div className="flex items-center justify-between">
          <div className="space-y-0.5">
            <h3 className="font-medium">Privacy Settings</h3>
            <p className="text-muted-foreground text-sm">
              Manage your privacy preferences
            </p>
          </div>
          <Button variant="outline">Edit</Button>
        </div>
      </CardContent>
      <CardFooter>
        <Button variant="destructive" className="w-full">
          <LogOut className="mr-2 h-4 w-4" />
          Sign Out
        </Button>
      </CardFooter>
    </Card>
  );
}
