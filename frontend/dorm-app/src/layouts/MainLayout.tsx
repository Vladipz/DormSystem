import { Navbar } from "@/components/Navbar";
import { Sidebar } from "@/components/Sidebar";
import { Outlet } from "@tanstack/react-router";

export function MainLayout() {
  return (
    <div className="h-screen flex overflow-hidden">
      {/* Sidebar column - fixed */}
      <Sidebar />

      {/* Main content column */}
      <div className="flex-1 flex flex-col h-screen">
        {/* Navbar - fixed */}
        <header className="min-h-[60px]">
          <Navbar />
        </header>
        {/* Scrollable main content */}
        <main className="flex-1 overflow-auto">
          <div className="container mx-auto px-4">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
