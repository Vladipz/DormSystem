import { RootRoute, Route, Router } from "@tanstack/react-router";
import { MainLayout } from "./layouts/MainLayout";
import HomePage from "./pages/home";

const rootRoute = new RootRoute({
  component: MainLayout,
});

const indexRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/",
  component: HomePage,
});

const residentsRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/residents",
  component: () => <div>Residents Page</div>,
});

const spacesRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/spaces",
  component: () => <div>Available Spaces Page</div>,
});

const eventsRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/events",
  component: () => <div>Events Page</div>,
});

const bookingsRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/bookings",
  component: () => <div>Room Bookings Page</div>,
});

const laundryRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/laundry",
  component: () => <div>Laundry Booking Page</div>,
});

const requestsRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/requests",
  component: () => <div>Purchase Requests Page</div>,
});

const marketplaceRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/marketplace",
  component: () => <div>Marketplace Page</div>,
});

const profileRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/profile",
  component: () => <div>Profile Page</div>,
});

const settingsRoute = new Route({
  getParentRoute: () => rootRoute,
  path: "/settings",
  component: () => <div>Settings Page</div>,
});

const routeTree = rootRoute.addChildren([
  indexRoute,
  residentsRoute,
  spacesRoute,
  eventsRoute,
  bookingsRoute,
  laundryRoute,
  requestsRoute,
  marketplaceRoute,
  profileRoute,
  settingsRoute,
]);

export const router = new Router({ routeTree });
