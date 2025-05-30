import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/lib/hooks/useAuth";
import { useLocation, useNavigate } from "@tanstack/react-router";
import {
  Building,
  Calendar,
  ClipboardCheck,
  Home,
  Shield,
  ShoppingCart,
  Users
} from "lucide-react";

interface NavigationItem {
  path: string;
  icon: React.ComponentType<{ className?: string }>;
  label: string;
  adminOnly?: boolean;
  comingSoon?: boolean;
}

// Implemented navigation items
export const navigationItems: NavigationItem[] = [
  { path: "/", icon: Home, label: "Overview" },
  { path: "/events", icon: Calendar, label: "Events" },
  { path: "/room-dashboard", icon: Building, label: "Room Dashboard" },
  { path: "/profile", icon: Users, label: "Profile" },
  { path: "/inspections", icon: ClipboardCheck, label: "Inspections" },
  { path: "/admin", icon: Shield, label: "Admin Panel", adminOnly: true },
];

// Coming soon navigation items
export const comingSoonItems: NavigationItem[] = [
  { path: "/bookings", icon: Calendar, label: "Room Bookings", comingSoon: true },
  { path: "/requests", icon: ShoppingCart, label: "Purchase Requests", comingSoon: true },
  { path: "/marketplace", icon: ShoppingCart, label: "Marketplace", comingSoon: true },
];

// Combined navigation items for use in components
export const allNavigationItems: NavigationItem[] = [...navigationItems, ...comingSoonItems];

interface NavigationItemsProps {
  onItemClick?: () => void;
  className?: string;
  showComingSoon?: boolean; // Option to show/hide coming soon items
}

export function NavigationItems({
  onItemClick,
  className = "",
  showComingSoon = true,
}: NavigationItemsProps) {
  const location = useLocation();
  const navigate = useNavigate();
  const { isAuthenticated, userRole } = useAuth();

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
      onItemClick?.();
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
      // Admin role checking is handled in the route's beforeLoad
      navigate({ to: "/inspections" });
      onItemClick?.();
      return;
    }

    // For other routes, proceed normally
    navigate({ to: path });
    onItemClick?.();
  };

  // Get items to display
  const itemsToShow = showComingSoon ? allNavigationItems : navigationItems;

  // Filter navigation items based on user role
  const filteredNavigationItems = itemsToShow.filter((item) => {
    if (item.adminOnly && userRole !== "Admin") {
      return false;
    }
    return true;
  });

  return (
    <nav className={`space-y-2 ${className}`}>
      {filteredNavigationItems.map((item) => {
        const Icon = item.icon;
        const isComingSoon = item.comingSoon;
        
        return (
          <Button
            key={item.path}
            variant={isRouteActive(item.path) ? "default" : "ghost"}
            className={`w-full justify-start ${isComingSoon ? "cursor-not-allowed opacity-70" : ""}`}
            onClick={(e) => handleNavClick(item.path, e, isComingSoon)}
            disabled={isComingSoon}
          >
            <div className="flex items-center justify-between w-full">
              <div className="flex items-center">
                <Icon className="mr-2 h-5 w-5" />
                {item.label}
              </div>
              {isComingSoon && (
                <Badge variant="secondary" className="text-xs ml-2">
                  Soon
                </Badge>
              )}
            </div>
          </Button>
        );
      })}
    </nav>
  );
}
