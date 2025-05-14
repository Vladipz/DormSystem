import axios from "axios";
import { jwtDecode } from "jwt-decode";
import { AuthUser, JwtPayload } from "../types/auth";

// Define base API URL from environment variable
const VITE_API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL ?? "http://localhost:5000";

class AuthService {
  private getDecodedToken(): JwtPayload | null {
    const token = localStorage.getItem("accessToken");
    if (!token) return null;

    try {
      return jwtDecode<JwtPayload>(token);
    } catch {
      return null;
    }
  }

  checkAuthStatus(): AuthUser | null {
    const decodedToken = this.getDecodedToken();
    if (!decodedToken) return null;

    // Перевірка чи токен не прострочений
    const isExpired = decodedToken.exp * 1000 < Date.now();
    if (isExpired) {
      this.logout();
      return null;
    }

    return {
      id: decodedToken[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
      ],
      role: decodedToken[
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
      ],
      firstName:
        decodedToken[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"
        ],
      lastName:
        decodedToken[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"
        ],
      isAuthenticated: true,
    };
  }

  async refreshToken(): Promise<boolean> {
    const refreshToken = localStorage.getItem("refreshToken");
    if (!refreshToken) return false;

    try {
      const response = await axios.post<{ accessToken: string; refreshToken: string }>(
        `${VITE_API_GATEWAY_URL}/api/auth/refresh`,
        { refreshToken }
      );

      localStorage.setItem("accessToken", response.data.accessToken);
      localStorage.setItem("refreshToken", response.data.refreshToken);
      return true;
    } catch {
      // Ignore the error details and just handle the failure
      this.logout();
      return false;
    }
  }

  logout(): void {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
  }

  isTokenExpired(): boolean {
    const decodedToken = this.getDecodedToken();
    if (!decodedToken) return true;
    
    // Add a 30-second buffer to account for timing variations
    return (decodedToken.exp * 1000) < (Date.now() + 30000);
  }
}

export const authService = new AuthService();
