import type { JwtPayload as BaseJwtPayload } from "jwt-decode";

export interface JwtPayload extends BaseJwtPayload {
  given_name: string;
  family_name: string;
  role: string;
}

export interface AuthUser {
  id: string;
  role: string;
  isAuthenticated: boolean;
  firstName?: string;
  lastName?: string;
}

export interface LinkCodeResponse {
  code: string;
  expiresAt: string;
}

export interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  avatarUrl?: string;
  roles: string[];
}

export interface AvatarUploadResponse {
  avatarUrl: string;
  message: string;
}