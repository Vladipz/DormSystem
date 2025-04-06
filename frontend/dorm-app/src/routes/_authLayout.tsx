import { createFileRoute, Outlet } from "@tanstack/react-router";

export const Route = createFileRoute("/_authLayout")({
  component: AuthLayout,
});

function AuthLayout() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <Outlet />
    </div>
  );
}
