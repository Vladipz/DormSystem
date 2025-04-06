import { Outlet } from "@tanstack/react-router";

export function AuthLayout() {
    return (
        <div className="min-h-screen bg-background">
            <main className="flex-1">
                <Outlet />
            </main>
        </div>
    );
}
