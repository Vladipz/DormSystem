import { Button } from "@/components/ui/button";
import { authService } from "@/lib/services/authService";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";

export function Navbar() {
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
        <h2 className="text-2xl font-bold">Dorm System</h2>
        <div className="flex gap-4">
          {isLoggedIn ? (
            <>
              <span className="flex items-center mr-2">
                Роль: {authData?.role || 'User'}
              </span>
              <Button onClick={handleLogout} disabled={logoutMutation.isPending}>
                {logoutMutation.isPending ? "Виходимо..." : "Вийти"}
              </Button>
            </>
          ) : (
            <>
              <Button variant="ghost" asChild>
                <Link to="/login">Увійти</Link>
              </Button>
              <Button asChild>
                <Link to="/register">Зареєструватися</Link>
              </Button>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
