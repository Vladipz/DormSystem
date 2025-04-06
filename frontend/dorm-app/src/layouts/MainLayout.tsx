import { Navbar } from "@/components/Navbar";
import { Outlet } from "@tanstack/react-router";

export function MainLayout() {
  return (
    <div className="min-h-screen flex flex-col">
      <div className="min-h-[60px]">
        <Navbar />
      </div>
      <main className="flex-1">
        <div className="container mx-auto px-4">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
