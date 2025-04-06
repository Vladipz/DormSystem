import { Button } from "@/components/ui/button";
import { usePageTitle } from "@/lib/hooks/usePageTitle";
import { authService } from "@/lib/services/authService";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";

export function Navbar() {
  const { pageTitle } = usePageTitle();
  const queryClient = useQueryClient();

  const { data: authData } = useQuery({
    queryKey: ["authStatus"],
    queryFn: authService.checkAuthStatus.bind(authService),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const isLoggedIn = !!authData?.isAuthenticated;

  const logoutMutation = useMutation({
    mutationFn: async () => {
      authService.logout();
      return { success: true };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["authStatus"] });
    },
  });

  const handleLogout = () => {
    logoutMutation.mutate();
  };

  return (
    <header className="w-full border-b bg-background z-50">
      <div className="container mx-auto px-4 py-3 flex justify-between items-center">
        <h2 className="text-2xl font-bold">{pageTitle}</h2>
        <div className="flex gap-4">
          {isLoggedIn ? (
            <>
              <span className="flex items-center mr-2">
                Role: {authData?.role || "User"}
              </span>
              <Button
                onClick={handleLogout}
                disabled={logoutMutation.isPending}
              >
                {logoutMutation.isPending ? "Loading..." : "Logout"}
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
