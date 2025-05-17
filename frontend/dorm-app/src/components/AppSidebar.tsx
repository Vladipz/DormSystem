import { Button } from "@/components/ui/button";
import { Link, useLocation } from "@tanstack/react-router";
import {
  Building,
  Calendar,
  Home,
  Settings,
  ShoppingCart,
  Users,
} from "lucide-react";

const navigationItems = [
  { path: "/", icon: Home, label: "Overview" },
  { path: "/events", icon: Calendar, label: "Events" },
  { path: "/room-dashboard", icon: Building, label: "Room Dashboard" },
  { path: "/bookings", icon: Calendar, label: "Room Bookings" },
  { path: "/requests", icon: ShoppingCart, label: "Purchase Requests" },
  { path: "/marketplace", icon: ShoppingCart, label: "Marketplace" },
  { path: "/profile", icon: Users, label: "Profile" },
  { path: "/inspections", icon: Calendar, label: "Inspections" },
];

//NOTE: plan
// const navigationItems = [
//   { path: "/", icon: Home, label: "Overview" },
//   { path: "/events", icon: Calendar, label: "Events" },
//   { path: "/room-dashboard", icon: Building, label: "Room Dashboard" },
//   { path: "/bookings", icon: Calendar, label: "Room Bookings" },
//   { path: "/requests", icon: ShoppingCart, label: "Purchase Requests" },
//   { path: "/marketplace", icon: ShoppingCart, label: "Marketplace" },
//   { path: "/profile", icon: Users, label: "Profile" },
// ];

// const navigationItems = [
//   { path: "/", icon: Home, label: "Overview" },
//   { path: "/residents", icon: Users, label: "Residents" },
//   { path: "/spaces", icon: Home, label: "Available Spaces" },
//   { path: "/events", icon: Calendar, label: "Events" },
//   { path: "/room-dashboard", icon: Building, label: "Room Dashboard" },
//   { path: "/bookings", icon: Calendar, label: "Room Bookings" },
//   { path: "/laundry", icon: Calendar, label: "Laundry Booking" },
//   { path: "/requests", icon: ShoppingCart, label: "Purchase Requests" },
//   { path: "/marketplace", icon: ShoppingCart, label: "Marketplace" },
//   { path: "/profile", icon: Users, label: "Profile" },
// ];

export function AppSidebar() {
  const location = useLocation();

  const isRouteActive = (path: string) => {
    return location.pathname === path;
  };

  return (
    <div className="bg-card hidden w-64 border-r p-4 shadow-lg md:block">
      <div className="flex h-full flex-col">
        <div className="mb-8 flex items-center">
          <Home className="text-primary mr-2 h-6 w-6" />
          <h1 className="text-xl font-bold">Dorm</h1>
        </div>

        <nav className="flex-1 space-y-2">
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
                  <Icon className="mr-2 h-5 w-5" />
                  {item.label}
                </Link>
              </Button>
            );
          })}
        </nav>

        <div className="border-t pt-4">
          <Button
            variant={isRouteActive("/settings") ? "default" : "ghost"}
            className="w-full justify-start"
            asChild
          >
            <Link to="/settings">
              <Settings className="mr-2 h-5 w-5" />
              Settings
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}
