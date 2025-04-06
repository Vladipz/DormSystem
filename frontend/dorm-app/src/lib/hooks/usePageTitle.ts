import { useRouterState } from "@tanstack/react-router";
import { useEffect, useState } from "react";

interface PageConfig {
  path: string;
  title: string;
  icon?: React.ComponentType;
}

const pages: PageConfig[] = [
  { path: "/", title: "Home" },
  { path: "/residents", title: "Residents" },
  { path: "/spaces", title: "Available Spaces" },
  { path: "/events", title: "Events" },
  { path: "/bookings", title: "Room Bookings" },
  { path: "/laundry", title: "Laundry Booking" },
  { path: "/requests", title: "Purchase Requests" },
  { path: "/marketplace", title: "Marketplace" },
  { path: "/profile", title: "Profile" },
  { path: "/settings", title: "Settings" },
  { path: "/login", title: "Login" }
];

export function usePageTitle() {
  const routerState = useRouterState();
  const [pageTitle, setPageTitle] = useState("Dorm");

  const currentPath = routerState.location.pathname;

  useEffect(() => {
    const page = pages.find((p) => p.path === currentPath);

    if (page) {
      setPageTitle(page.title);
      document.title = `${page.title} | Dorm`;
    } else {
      setPageTitle("Dorm");
      document.title = "Dorm";
    }
  }, [currentPath]);

  return {
    pageTitle,
    PageIcon: pages.find((p) => p.path === currentPath)?.icon,
  };
}
