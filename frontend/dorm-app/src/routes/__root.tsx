import { Navbar } from "@/components/Navbar";
import { Sidebar } from "@/components/Sidebar";
import { createRootRoute, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

export const Route = createRootRoute({
  component: () => {
    const isAuthRoute = location.pathname.startsWith("/auth");

    if (isAuthRoute) {
      return (
        <>
          <Outlet />
          <TanStackRouterDevtools />
        </>
      );
    }

    return (
      <div className="h-screen flex overflow-hidden">
        <Sidebar />
        <div className="flex-1 flex flex-col h-screen">
          <header className="min-h-[60px]">
            <Navbar />
          </header>
          <main className="flex-1 overflow-auto">
            <div className="container mx-auto px-4">
              <Outlet />
            </div>
          </main>
        </div>
        <TanStackRouterDevtools />
      </div>
    );
  },
});
