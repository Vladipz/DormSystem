import { Button } from "@/components/ui/button";
import { useAuth } from "@/lib/hooks/useAuth";
import { useLocation, useNavigate } from "@tanstack/react-router";
import {
  Building,
  Calendar,
  ClipboardCheck,
  Home,
  Settings,
  ShoppingCart,
  Users,
} from "lucide-react";

export const navigationItems = [
  { path: "/", icon: Home, label: "Overview" },
  { path: "/events", icon: Calendar, label: "Events" },
  { path: "/room-dashboard", icon: Building, label: "Room Dashboard" },
  { path: "/bookings", icon: Calendar, label: "Room Bookings" },
  { path: "/requests", icon: ShoppingCart, label: "Purchase Requests" },
  { path: "/marketplace", icon: ShoppingCart, label: "Marketplace" },
  { path: "/profile", icon: Users, label: "Profile" },
  { path: "/inspections", icon: ClipboardCheck, label: "Inspections" },
  { path: "/settings", icon: Settings, label: "Settings" },
];

interface NavigationItemsProps {
  onItemClick?: () => void;
  className?: string;
}

export function NavigationItems({
  onItemClick,
  className = "",
}: NavigationItemsProps) {
  const location = useLocation();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

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
      // Admin role checking is handled in the route's beforeLoad
      navigate({ to: "/inspections" });
      onItemClick?.();
      return;
    }

    // For other routes, proceed normally
    navigate({ to: path });
    onItemClick?.();
  };

  return (
    <nav className={`space-y-2 ${className}`}>
      {navigationItems.map((item) => {
        const Icon = item.icon;
        return (
          <Button
            key={item.path}
            variant={isRouteActive(item.path) ? "default" : "ghost"}
            className="w-full justify-start"
            onClick={(e) => handleNavClick(item.path, e)}
          >
            <div className="flex items-center">
              <Icon className="mr-2 h-5 w-5" />
              {item.label}
            </div>
          </Button>
        );
      })}
    </nav>
  );
}
