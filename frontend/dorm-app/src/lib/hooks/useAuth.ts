import { useNavigate } from "@tanstack/react-router";
import { useCallback, useEffect, useState } from "react";
import { authService } from "../services/authService";
import { AuthUser } from "../types/auth";

/**
 * Custom hook for handling authentication state and operations
 */
export function useAuth() {
  const [user, setUser] = useState<AuthUser | null>(() =>
    authService.checkAuthStatus()
  );
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const navigate = useNavigate();

  // Check authentication status on mount
  useEffect(() => {
    const checkAuth = () => {
      try {
        const authStatus = authService.checkAuthStatus();
        setUser(authStatus);
      } finally {
        setIsLoading(false);
      }
    };

    checkAuth();

    // Optional: Set up a timer to periodically check token validity
    const intervalId = setInterval(checkAuth, 60000); // Check every minute

    return () => clearInterval(intervalId);
  }, []);

  /**
   * Logout function that handles both auth service and React state
   */
  const logout = useCallback(() => {
    authService.logout();
    setUser(null);
    navigate({ to: "/login" });
  }, [navigate]);

  /**
   * Refresh authentication status
   */
  const refreshAuth = useCallback(() => {
    const authStatus = authService.checkAuthStatus();
    setUser(authStatus);
    return authStatus;
  }, []);

  /**
   * Require authentication, redirects to login if not authenticated
   * @param returnTo Optional path to return to after login
   * @returns True if authenticated, false otherwise
   */
  const requireAuth = useCallback(
    (returnTo?: string) => {
      const currentAuth = refreshAuth();

      if (!currentAuth?.isAuthenticated) {
        navigate({
          to: "/login",
          search: returnTo ? { returnTo } : undefined,
        });
        return false;
      }

      return true;
    },
    [navigate, refreshAuth]
  );

  return {
    user,
    isLoading,
    isAuthenticated: !!user?.isAuthenticated,
    userId: user?.id,
    userRole: user?.role,
    logout,
    refreshAuth,
    requireAuth,
  };
}
