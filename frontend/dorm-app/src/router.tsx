import { Outlet, RootRoute, Route, Router } from "@tanstack/react-router";
import { AuthLayout } from "./layouts/AuthLayout";
import { MainLayout } from "./layouts/MainLayout";
import EventsPage from "./pages/Events/events";
import HomePage from "./pages/home";
import LoginPage from "./pages/login";

const rootRoute = new RootRoute({
  component: () => <Outlet />, // Використовуємо Outlet для відображення дочірніх роутів
});

// 2. Створюємо окремий роут для основного лейаута
const mainLayoutRoute = new Route({
  getParentRoute: () => rootRoute,
  id: "main",
  component: MainLayout,
});

// 3. Створюємо роут для auth лейаута на тому ж рівні
const authLayoutRoute = new Route({
  getParentRoute: () => rootRoute,
  id: "auth",
  component: AuthLayout,
});

const indexRoute = new Route({
  getParentRoute: () => mainLayoutRoute, // Змінюємо parent на mainLayoutRoute
  path: "/",
  component: HomePage,
});

// Fix loginRoute to use authLayoutRoute as parent
const loginRoute = new Route({
  getParentRoute: () => authLayoutRoute,
  path: "login",
  component: LoginPage,
});

const residentsRoute = new Route({
  getParentRoute: () => mainLayoutRoute,
  path: "/residents",
  component: () => <div>Residents Page</div>,
});

const spacesRoute = new Route({
  getParentRoute: () => mainLayoutRoute,
  path: "/spaces",
  component: () => <div>Available Spaces Page</div>,
});

const eventsRoute = new Route({
  getParentRoute: () => mainLayoutRoute,
  path: "/events",
  component: EventsPage,
});

const bookingsRoute = new Route({
  getParentRoute: () => mainLayoutRoute,
  path: "/bookings",
  component: () => <div>Room Bookings Page</div>,
});

const laundryRoute = new Route({
  getParentRoute: () => mainLayoutRoute,
  path: "/laundry",
  component: () => <div>Laundry Booking Page</div>,
});

const requestsRoute = new Route({
  getParentRoute: () => mainLayoutRoute,
  path: "/requests",
  component: () => <div>Purchase Requests Page</div>,
});

const marketplaceRoute = new Route({
  getParentRoute: () => mainLayoutRoute,
  path: "/marketplace",
  component: () => <div>Marketplace Page</div>,
});

const profileRoute = new Route({
  getParentRoute: () => mainLayoutRoute,
  path: "/profile",
  component: () => <div>Profile Page</div>,
});

const settingsRoute = new Route({
  getParentRoute: () => mainLayoutRoute,
  path: "/settings",
  component: () => <div>Settings Page</div>,
});

const routeTree = rootRoute.addChildren([
  mainLayoutRoute.addChildren([
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
  ]),
  authLayoutRoute.addChildren([loginRoute]),
]);

export const router = new Router({ routeTree });
