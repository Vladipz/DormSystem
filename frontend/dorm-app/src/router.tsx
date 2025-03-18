import {
    createRootRoute,
    createRoute,
    createRouter,
} from "@tanstack/react-router";
import { lazy } from "react";

// Lazy load pages
const HomePage = lazy(() => import("./pages/home"));
const LoginPage = lazy(() => import("./pages/login"));
const RegisterPage = lazy(() => import("./pages/register"));

// Create routes
const rootRoute = createRootRoute({
  component: () => (
    <div className="min-h-screen bg-background">
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  ),
});

// Import Outlet from react-router
import { Outlet } from "@tanstack/react-router";

// Create routes
const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/",
  component: HomePage,
});

const loginRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/login",
  component: LoginPage,
});

const registerRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/register",
  component: RegisterPage,
});

// Build the route tree
const routeTree = rootRoute.addChildren([
  indexRoute,
  loginRoute,
  registerRoute,
]);

// Create the router
export const router = createRouter({
  routeTree,
  defaultPreload: "intent",
  // Add this to show a loading indicator for code-split route components
  defaultPendingComponent: () => (
    <div className="flex items-center justify-center h-screen">
      <div className="animate-spin rounded-full h-10 w-10 border-t-2 border-b-2 border-primary"></div>
    </div>
  ),
});

// Register router types
declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}
