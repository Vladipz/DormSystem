import { ModeToggle } from "@/components/mode-toggle";
import { allNavigationItems } from "@/components/NavigationItems";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/lib/hooks/useAuth";
import { useMyNotifications } from "@/lib/hooks/useNotification";
import { Link, useLocation, useNavigate } from "@tanstack/react-router";
import { Bell, Menu } from "lucide-react";
import { useState } from "react";
import { Separator } from "./ui/separator";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "./ui/sheet";

// Main navigation items for bottom bar (4 most important implemented items)
const mainNavPaths = ["/", "/events", "/room-dashboard", "/profile"];
const mobileNotificationsItem = {
  path: "/notifications",
  icon: Bell,
  label: "Messages",
  comingSoon: false,
};

export function MobileBottomNav() {
  const location = useLocation();
  const navigate = useNavigate();
  const { isAuthenticated, userRole, user } = useAuth();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const { data: notificationData } = useMyNotifications(user?.id, 5);
  const unreadCount = notificationData?.unreadCount ?? 0;

  // Filter navigation items based on user role
  const filteredNavigationItems = allNavigationItems.filter((item) => {
    if (item.adminOnly && userRole !== "Admin") {
      return false;
    }
    return true;
  });

  // Filter navigation items
  const mainNavItems = filteredNavigationItems.filter(item => mainNavPaths.includes(item.path));
  const menuNavItems = [
    mobileNotificationsItem,
    ...filteredNavigationItems.filter(item => !mainNavPaths.includes(item.path)),
  ];

  const isRouteActive = (path: string) => {
    return location.pathname === path;
  };

  const handleNavClick = (path: string, e: React.MouseEvent, isComingSoon?: boolean) => {
    // Prevent navigation for coming soon items
    if (isComingSoon) {
      e.preventDefault();
      return;
    }

    // Special handling for admin route
    if (path === "/admin") {
      e.preventDefault();

      if (!isAuthenticated) {
        navigate({ to: "/login", search: { returnTo: "/admin" } });
        return;
      }

      if (userRole !== "Admin") {
        // Optionally show a toast or alert here
        return;
      }

      navigate({ to: "/admin" });
      setIsMenuOpen(false);
      return;
    }

    // Special handling for inspections route
    if (path === "/inspections") {
      e.preventDefault();
      
      if (!isAuthenticated) {
        navigate({ to: "/login", search: { returnTo: "/inspections" } });
        return;
      }
      
      // Allow access to inspections list for all authenticated users
      navigate({ to: "/inspections" });
      setIsMenuOpen(false);
      return;
    }

    // For other routes, navigate normally
    navigate({ to: path });
    setIsMenuOpen(false);
  };

  // Check if any menu item is active to highlight the menu button
  const isMenuItemActive = menuNavItems.some((item) =>
    isRouteActive(item.path)
  );

  return (
    <div className="md:hidden fixed bottom-0 left-0 right-0 z-50 bg-background border-t pb-safe">
      <nav className="flex items-center justify-around px-2 py-2 pb-2">
        {/* Main navigation buttons */}
        {mainNavItems.map((item) => {
          const Icon = item.icon;
          const isActive = isRouteActive(item.path);
          const isComingSoon = item.comingSoon;
          
          if (isComingSoon) {
            return (
              <Button
                key={item.path}
                variant="ghost"
                className="flex flex-col items-center justify-center p-2 min-w-0 flex-1 h-auto cursor-not-allowed opacity-70"
                disabled
              >
                <div className="flex flex-col items-center justify-center gap-1 text-muted-foreground">
                  <Icon className="h-5 w-5" />
                  <span className="text-xs font-medium truncate max-w-full">
                    {item.label}
                  </span>
                  <Badge variant="secondary" className="text-xs mt-1">
                    Soon
                  </Badge>
                </div>
              </Button>
            );
          }
          
          return (
            <Link
              key={item.path}
              to={item.path}
              className="flex flex-col items-center justify-center p-2 min-w-0 flex-1"
            >
              <div
                className={`flex flex-col items-center justify-center gap-1 transition-colors ${
                  isActive
                    ? "text-primary"
                    : "text-muted-foreground hover:text-foreground"
                }`}
              >
                <Icon className="h-5 w-5" />
                <span className="text-xs font-medium truncate max-w-full">
                  {item.label}
                </span>
              </div>
            </Link>
          );
        })}

        {/* Menu button */}
        <Sheet open={isMenuOpen} onOpenChange={setIsMenuOpen}>
          <SheetTrigger asChild>
            <Button
              variant="ghost"
              className="flex flex-col items-center justify-center p-2 min-w-0 flex-1 h-auto"
            >
              <div
                className={`relative flex flex-col items-center justify-center gap-1 transition-colors ${
                  isMenuItemActive
                    ? "text-primary"
                    : "text-muted-foreground hover:text-foreground"
                }`}
              >
                <Menu className="h-5 w-5" />
                {unreadCount > 0 && (
                  <span className="absolute -top-1 left-1/2 ml-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[9px] font-semibold text-white">
                    {unreadCount > 9 ? "9+" : unreadCount}
                  </span>
                )}
                <span className="text-xs font-medium">More</span>
              </div>
            </Button>
          </SheetTrigger>
          <SheetContent side="bottom" className="h-auto max-h-[80vh]">
            <SheetHeader>
              <SheetTitle>More Options</SheetTitle>
              <SheetDescription>
                Access additional features and settings
              </SheetDescription>
            </SheetHeader>
            <div className="grid gap-2 py-4">
              {menuNavItems.map((item) => {
                const Icon = item.icon;
                const isComingSoon = item.comingSoon;
                
                return (
                  <Button
                    key={item.path}
                    variant={isRouteActive(item.path) ? "default" : "ghost"}
                    className={`w-full justify-start h-12 ${isComingSoon ? "cursor-not-allowed opacity-70" : ""}`}
                    onClick={(e) => handleNavClick(item.path, e, isComingSoon)}
                    disabled={isComingSoon}
                  >
                    <div className="flex items-center justify-between w-full">
                      <div className="flex items-center">
                        <Icon className="mr-3 h-5 w-5" />
                        {item.label}
                      </div>
                      <div className="flex items-center gap-2">
                        {item.path === "/notifications" && unreadCount > 0 && (
                          <Badge variant="destructive" className="min-w-5 px-1 text-[10px]">
                            {unreadCount > 9 ? "9+" : unreadCount}
                          </Badge>
                        )}
                        {isComingSoon && (
                          <Badge variant="secondary" className="ml-2 text-xs">
                            Soon
                          </Badge>
                        )}
                      </div>
                    </div>
                  </Button>
                );
              })}
              <Separator className="my-2" />
              <div className="flex items-center justify-between px-3 py-2">
                <span className="text-sm font-medium">Theme</span>
                <ModeToggle />
              </div>
            </div>
          </SheetContent>
        </Sheet>
      </nav>
    </div>
  );
} 
