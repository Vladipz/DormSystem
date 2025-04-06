import { Button } from "@/components/ui/button";
import { Link, useLocation } from "@tanstack/react-router";
import { Calendar, Home, Settings, ShoppingCart, Users } from "lucide-react";

const navigationItems = [
  { path: "/", icon: Home, label: "Overview" },
  { path: "/residents", icon: Users, label: "Residents" },
  { path: "/spaces", icon: Home, label: "Available Spaces" },
  { path: "/events", icon: Calendar, label: "Events" },
  { path: "/bookings", icon: Calendar, label: "Room Bookings" },
  { path: "/laundry", icon: Calendar, label: "Laundry Booking" },
  { path: "/requests", icon: ShoppingCart, label: "Purchase Requests" },
  { path: "/marketplace", icon: ShoppingCart, label: "Marketplace" },
  { path: "/profile", icon: Users, label: "Profile" },
];

export function Sidebar() {
  const location = useLocation();

  const isRouteActive = (path: string) => {
    return location.pathname === path;
  };

  return (
    <div className="w-64 border-r bg-card p-4 shadow-lg">
      <div className="flex flex-col h-full">
        <div className="flex items-center mb-8">
          <Home className="w-6 h-6 mr-2 text-primary" />
          <h1 className="text-xl font-bold">Dorm</h1>
        </div>

        <nav className="space-y-2 flex-1">
          {navigationItems.map((item) => {
            const Icon = item.icon;
            return (
              <Button
                key={item.path}
                variant={isRouteActive(item.path) ? "default" : "ghost"}
                className="w-full justify-start"
                asChild
              >
                <Link to={item.path}>
                  <Icon className="w-5 h-5 mr-2" />
                  {item.label}
                </Link>
              </Button>
            );
          })}
        </nav>

        <div className="pt-4 border-t">
          <Button
            variant={isRouteActive("/settings") ? "default" : "ghost"}
            className="w-full justify-start"
            asChild
          >
            <Link to="/settings">
              <Settings className="w-5 h-5 mr-2" />
              Settings
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}
