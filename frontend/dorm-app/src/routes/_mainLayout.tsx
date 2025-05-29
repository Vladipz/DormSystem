import { AppSidebar } from "@/components/AppSidebar";
import { MobileBottomNav } from "@/components/MobileBottomNav";
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
    <div className="flex h-screen overflow-hidden">
      <AppSidebar />
      <div className="flex h-screen flex-1 flex-col">
        <header className="min-h-[60px]">
          <Navbar />
        </header>
        <main className="flex-1 overflow-auto" style={{ scrollbarGutter: "stable" }}>
          {/* Add bottom padding on mobile to account for fixed bottom navigation + safe area */}
          <div className="container mx-auto px-4 py-4 pb-24 md:pb-4">
            <Outlet />
          </div>
        </main>
      </div>
      {/* Mobile bottom navigation */}
      <MobileBottomNav />
    </div>
  );
}
