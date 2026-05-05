import { useNavigate } from "@tanstack/react-router";
import {
  createContext,
  createElement,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { authService } from "../services/authService";
import { AuthUser, LinkCodeResponse } from "../types/auth";

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  userId: string | undefined;
  userRole: string | undefined;
  logout: () => Promise<void>;
  refreshAuth: () => Promise<AuthUser | null>;
  requireAuth: (returnTo?: string) => Promise<boolean>;
  generateLinkCode: () => Promise<LinkCodeResponse>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

/**
 * Keeps auth state shared across the app instead of re-checking auth in every consumer.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const navigate = useNavigate();

  useEffect(() => {
    let isMounted = true;

    const checkAuth = async () => {
      try {
        const authStatus = await authService.checkAuthStatus();
        if (isMounted) {
          setUser(authStatus);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    void checkAuth();

    return () => {
      isMounted = false;
    };
  }, []);

  const logout = useCallback(async () => {
    await authService.logout();
    setUser(null);
    navigate({ to: "/" });
  }, [navigate]);

  const refreshAuth = useCallback(async () => {
    const authStatus = await authService.checkAuthStatus();
    setUser(authStatus);
    return authStatus;
  }, []);

  const requireAuth = useCallback(
    async (returnTo?: string) => {
      const currentAuth = await refreshAuth();

      if (!currentAuth?.isAuthenticated) {
        navigate({ to: "/login", search: { returnTo: returnTo ?? "/" } });
        return false;
      }

      return true;
    },
    [navigate, refreshAuth],
  );

  const generateLinkCode = useCallback(async (): Promise<LinkCodeResponse> => {
    return await authService.generateLinkCode();
  }, []);

  const value: AuthContextValue = {
    user,
    isLoading,
    isAuthenticated: !!user?.isAuthenticated,
    userId: user?.id,
    userRole: user?.role,
    logout,
    refreshAuth,
    requireAuth,
    generateLinkCode,
  };

  return createElement(AuthContext.Provider, { value }, children);
}

/**
 * Custom hook for handling authentication state and operations
 */
export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within AuthProvider");
  }

  return context;
}
