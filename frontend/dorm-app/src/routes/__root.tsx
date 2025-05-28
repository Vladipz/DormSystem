import { createRootRoute, Outlet } from "@tanstack/react-router";
import { Toaster } from "sonner";

export const Route = createRootRoute({
  component: () => (
    <>
      <Toaster position="bottom-right" richColors />
      <Outlet />
      {/* <TanStackRouterDevtools /> */}
    </>
  ),
});
