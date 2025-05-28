import { navigationItems } from "@/components/NavigationItems";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/lib/hooks/useAuth";
import { Link, useLocation, useNavigate } from "@tanstack/react-router";
import { Menu } from "lucide-react";
import { useState } from "react";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "./ui/sheet";

// Main navigation items for bottom bar (4 most important)
const mainNavPaths = ["/", "/events", "/room-dashboard", "/profile"];

// Filter navigation items
const mainNavItems = navigationItems.filter(item => mainNavPaths.includes(item.path));
const menuNavItems = navigationItems.filter(item => !mainNavPaths.includes(item.path));

export function MobileBottomNav() {
  const location = useLocation();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const isRouteActive = (path: string) => {
    return location.pathname === path;
  };

  const handleNavClick = (path: string, e: React.MouseEvent) => {
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
                className={`flex flex-col items-center justify-center gap-1 transition-colors ${
                  isMenuItemActive
                    ? "text-primary"
                    : "text-muted-foreground hover:text-foreground"
                }`}
              >
                <Menu className="h-5 w-5" />
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
                return (
                  <Button
                    key={item.path}
                    variant={isRouteActive(item.path) ? "default" : "ghost"}
                    className="w-full justify-start h-12"
                    onClick={(e) => handleNavClick(item.path, e)}
                  >
                    <Icon className="mr-3 h-5 w-5" />
                    {item.label}
                  </Button>
                );
              })}
            </div>
          </SheetContent>
        </Sheet>
      </nav>
    </div>
  );
} 