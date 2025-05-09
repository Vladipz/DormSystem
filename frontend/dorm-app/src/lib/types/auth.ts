export interface JwtPayload {
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname": string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname": string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string;
  
  exp: number;
  iss: string;
  aud: string;
}

export interface AuthUser {
  id: string;
  role: string;
  isAuthenticated: boolean;
  firstName?: string;
  lastName?: string;
}