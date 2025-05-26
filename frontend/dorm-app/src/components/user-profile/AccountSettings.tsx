import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";

import { useAuth } from "@/lib/hooks/useAuth";
import { Copy, LogOut, MessageSquare } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

export function AccountSettings() {
  const { generateLinkCode, logout } = useAuth();
  const [linkCode, setLinkCode] = useState<string>("");
  const [isGenerating, setIsGenerating] = useState(false);
  const [expiresAt, setExpiresAt] = useState<string>("");

  const handleGenerateLinkCode = async () => {
    setIsGenerating(true);
    try {
      const response = await generateLinkCode();
      setLinkCode(response.code);
      setExpiresAt(response.expiresAt);
      // Success notification could be added here
    } catch {
      alert("Failed to generate link code. Please try again.");
    } finally {
      setIsGenerating(false);
    }
  };

  const handleCopyCode = async () => {
    if (linkCode) {
      try {
        await navigator.clipboard.writeText(linkCode);
        toast.success("Code copied to clipboard");
      } catch {
        toast.error("Failed to copy code to clipboard.");
      }
    }
  };

  const handleLogout = () => {
    logout();
  };

  const formatExpiryTime = (expiresAt: string) => {
    if (!expiresAt) return "";
    const date = new Date(expiresAt);
    return date.toLocaleString();
  };

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
        <div className="space-y-3">
          <div className="space-y-0.5">
            <h3 className="font-medium">Link Telegram Account</h3>
            <p className="text-muted-foreground text-sm">
              Generate a code to link your Telegram account for notifications
            </p>
          </div>
          <div className="flex gap-2">
            <Button 
              onClick={handleGenerateLinkCode} 
              disabled={isGenerating}
              className="flex-shrink-0"
            >
              <MessageSquare className="mr-2 h-4 w-4" />
              {isGenerating ? "Generating..." : "Generate Code"}
            </Button>
          </div>
          {linkCode && (
            <div className="space-y-2">
              <div className="flex gap-2">
                <Input 
                  value={linkCode} 
                  readOnly 
                  className="font-mono text-center text-lg tracking-wider"
                />
                <Button 
                  onClick={handleCopyCode} 
                  variant="outline" 
                  size="icon"
                  className="flex-shrink-0"
                >
                  <Copy className="h-4 w-4" />
                </Button>
              </div>
              <p className="text-xs text-muted-foreground">
                Code expires at: {formatExpiryTime(expiresAt)}
              </p>
              <p className="text-xs text-muted-foreground">
                Send this code to the Telegram bot using: <code>/auth {linkCode}</code>
              </p>
            </div>
          )}
        </div>
      </CardContent>
      <CardFooter>
        <Button variant="destructive" className="w-full" onClick={handleLogout}>
          <LogOut className="mr-2 h-4 w-4" />
          Sign Out
        </Button>
      </CardFooter>
    </Card>
  );
}
