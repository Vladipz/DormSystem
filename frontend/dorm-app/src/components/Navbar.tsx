import { Button } from "@/components/ui/button";
import { useAuth } from "@/lib/hooks/useAuth"; // Імпортуємо useAuth
import { usePageTitle } from "@/lib/hooks/usePageTitle";
import { Link } from "@tanstack/react-router";
import { useState } from "react";

export function Navbar() {
  const { pageTitle } = usePageTitle();
  const { user, isAuthenticated, isLoading, logout } = useAuth(); // Використовуємо useAuth
  const [error, setError] = useState<string | null>(null);

  const handleLogout = async () => {
    try {
      await logout();
    } catch {
      setError("Failed to logout. Please try again.");
      // Auto-clear error after 3 seconds
      setTimeout(() => setError(null), 3000);
    }
  };

  return (
    <header className="bg-background z-50 w-full border-b">
      <div className="container mx-auto flex items-center justify-between px-4 py-3">
        <h2 className="text-2xl font-bold">{pageTitle}</h2>
        <div className="flex gap-4">
          {error && (
            <span className="self-center text-sm text-red-500">{error}</span>
          )}

          {isLoading ? (
            <span>Loading...</span> // Показуємо лоадер під час перевірки
          ) : isAuthenticated && user ? (
            <>
              <span className="mr-2 flex items-center">
                Role: {user.role || "User"} {user.firstName || "User"}{" "}
                {user.lastName || "User"}
              </span>
              <Button onClick={handleLogout} disabled={isLoading}>
                {isLoading ? "Loading..." : "Logout"}
              </Button>
            </>
          ) : (
            <>
              <Button variant="ghost" asChild>
                <Link to="/login">Login</Link>
              </Button>
              <Button asChild>
                <Link to="/register">Register</Link>
              </Button>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
