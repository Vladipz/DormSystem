import axios from "axios";
import { jwtDecode } from "jwt-decode";
import { AuthUser, JwtPayload } from "../types/auth";

// Define base API URL from environment variable
const VITE_API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL ?? "http://localhost:5000";

class AuthService {
  private isRefreshing = false;
  private refreshPromise: Promise<boolean> | null = null;

  private getDecodedToken(): JwtPayload | null {
    const token = localStorage.getItem("accessToken");
    if (!token) return null;

    try {
      return jwtDecode<JwtPayload>(token);
    } catch {
      return null;
    }
  }

  isTokenExpired(): boolean {
    const decodedToken = this.getDecodedToken();
    if (!decodedToken) return true;
    
    return decodedToken.exp * 1000 < Date.now();
  }

  async checkAuthStatus(): Promise<AuthUser | null> {
    // Отримуємо токен
    const token = localStorage.getItem("accessToken");
    if (!token) return null;
    
    // Перевіряємо валідність
    if (this.isTokenExpired()) {
      // Токен простроченний, пробуємо оновити
      const refreshed = await this.refreshToken();
      if (!refreshed) {
        // Не вдалося оновити, виходимо
        this.logout();
        return null;
      }
    }
    
    // Отримуємо дані з токена (який може бути оновленим)
    const decodedToken = this.getDecodedToken();
    if (!decodedToken) return null;

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

    // Якщо вже йде процес оновлення токену, повертаємо існуючий проміс
    if (this.isRefreshing && this.refreshPromise) {
      return this.refreshPromise;
    }

    // Створюємо новий проміс для оновлення токену
    this.isRefreshing = true;
    this.refreshPromise = new Promise<boolean>((resolve) => {
      const refreshAsync = async () => {
        try {
          const response = await axios.post<{ accessToken: string; refreshToken: string }>(
            `${VITE_API_GATEWAY_URL}/api/auth/refresh`,
            { refreshToken }
          );

          localStorage.setItem("accessToken", response.data.accessToken);
          localStorage.setItem("refreshToken", response.data.refreshToken);
          resolve(true);
        } catch {
          // Ignore the error details and just handle the failure
          this.logout();
          resolve(false);
        } finally {
          this.isRefreshing = false;
          this.refreshPromise = null;
        }
      };
      
      refreshAsync();
    });

    return this.refreshPromise;
  }

  logout(): void {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
  }
}

export const authService = new AuthService();
