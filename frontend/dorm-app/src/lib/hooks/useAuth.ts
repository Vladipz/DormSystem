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
    const checkAuth = async () => {
      try {
        const authStatus = authService.checkAuthStatus();
        setUser(authStatus);
      } finally {
        setIsLoading(false);
      }
    };

    checkAuth();
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
  const refreshAuth = useCallback(async () => {
    // Only attempt to refresh token when explicitly called
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
    async (returnTo?: string) => {
      const currentAuth = await refreshAuth();

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
