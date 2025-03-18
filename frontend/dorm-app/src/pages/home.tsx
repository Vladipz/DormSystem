import { Button } from "@/components/ui/button";
import { Link } from "@tanstack/react-router";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

// Auth service function (you'd typically place this in a separate service file)
const checkAuthStatus = async () => {

  const token = localStorage.getItem("accessToken");
  if (!token) return null;

  // In a real app, you might validate the token with your API
  // return await apiClient.get('/auth/validate-token');
  return { isAuthenticated: true };
};

export default function HomePage() {
  const queryClient = useQueryClient();

  // Query for auth status
  const { data: authData } = useQuery({
    queryKey: ['authStatus'],
    queryFn: checkAuthStatus,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const isLoggedIn = !!authData?.isAuthenticated;

  // Mutation for logout
  const logoutMutation = useMutation({
    mutationFn: async () => {
      localStorage.removeItem("accessToken");
      // In a real app, you might also call an API endpoint
      // return await apiClient.post('/auth/logout');
      return { success: true };
    },
    onSuccess: () => {
      // Invalidate and refetch auth status after logout
      queryClient.invalidateQueries({ queryKey: ['authStatus'] });
    },
  });

  const handleLogout = () => {
    logoutMutation.mutate();
  };

  return (
    <div className="flex flex-col min-h-screen">
      {/* Navbar */}
      <header className="w-full border-b">
        <div className="container mx-auto px-4 py-3 flex justify-between items-center">
          <h2 className="text-2xl font-bold">Dorm System</h2>
          <div className="flex gap-4">
            {isLoggedIn ? (
              <Button
                onClick={handleLogout}
                disabled={logoutMutation.isPending}
              >
                {logoutMutation.isPending ? "Виходимо..." : "Вийти"}
              </Button>
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

      {/* Main content */}
      <main className="flex-1 flex flex-col items-center justify-center p-4 space-y-8">
        <h1 className="text-4xl font-bold text-center">Welcome to Dorm System</h1>
        <p className="text-center text-lg max-w-xl">
          Система управління гуртожитками допомагає студентам та адміністрації ефективно взаємодіяти.
          Реєструйтесь для повного доступу до функцій системи.
        </p>
        {!isLoggedIn && (
          <div className="flex gap-4">
            <Button asChild>
              <Link to="/login">Увійти</Link>
            </Button>
            <Button variant="outline" asChild>
              <Link to="/register">Зареєструватися</Link>
            </Button>
          </div>
        )}
      </main>
    </div>
  );
}
