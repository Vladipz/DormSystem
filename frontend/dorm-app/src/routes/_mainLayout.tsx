import { AppSidebar } from "@/components/AppSidebar";
import { Navbar } from "@/components/Navbar";
import { createFileRoute, Outlet } from "@tanstack/react-router";
export const Route = createFileRoute("/_mainLayout")({
  component: RouteComponent,
});

function RouteComponent() {
  // Перевіряємо, чи це auth route
  const isAuthRoute = location.pathname.startsWith("/auth");

  if (isAuthRoute) {
    return <Outlet />;
  }

  return (
    <div className="h-screen flex overflow-hidden">
      <AppSidebar />
      <div className="flex-1 flex flex-col h-screen">
        <header className="min-h-[60px]">
          <Navbar />
        </header>
        <main className="flex-1 overflow-auto">
          <div className="container mx-auto px-4 py-4">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
