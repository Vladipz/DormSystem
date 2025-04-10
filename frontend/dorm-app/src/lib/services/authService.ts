import { jwtDecode } from "jwt-decode";
import { AuthUser, JwtPayload } from "../types/auth";

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

  logout(): void {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
  }
}

export const authService = new AuthService();
