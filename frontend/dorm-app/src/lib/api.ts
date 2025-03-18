// Base API URL - replace with your actual API URL
const API_BASE_URL = import.meta.env.VITE_API_GATEWAY_URL || '';

// Type for API errors
interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}

// Helper function to handle API responses
async function handleResponse<T>(response: Response): Promise<T> {
  // Special handling for 404 errors to capture raw text
  if (response.status === 404) {
    const textError = await response.text();
    const error: ApiError = {
      message: textError || 'Resource not found',
    };
    throw error;
  }

  try {
    const data = await response.json();

    if (!response.ok) {
      const error: ApiError = {
        message: data.message || 'An error occurred',
        errors: data.errors,
      };
      throw error;
    }

    return data as T;
  } catch (error) {
    if (error instanceof SyntaxError) {
      // This catches JSON parse errors
      throw new Error('Invalid response from server');
    }
    throw error;
  }
}
export function generateCodeVerifier(): string {
  const array = new Uint8Array(32); // 32 байти для безпечного коду
  window.crypto.getRandomValues(array); // Генеруємо криптографічно безпечний випадковий масив
  return btoa(String.fromCharCode(...array)) // Перетворюємо в Base64
    .replace(/\+/g, '-') // Замінюємо '+' на '-'
    .replace(/\//g, '_') // Замінюємо '/' на '_'
    .replace(/=/g, ''); // Видаляємо '='
}


export async function generateCodeChallenge(codeVerifier: string): Promise<string> {
  const encoder = new TextEncoder();
  const data = encoder.encode(codeVerifier);
  const digest = await crypto.subtle.digest('SHA-256', data);
  const base64 = btoa(String.fromCharCode(...new Uint8Array(digest)))
    .replace(/=/g, '') // Remove padding
    .replace(/\+/g, '-') // Make URL-safe
    .replace(/\//g, '_'); // Make URL-safe
  return base64;
}

// Auth request interfaces
interface AuthorizeRequest {
  email: string;
  password: string;
  codeChallenge: string;
}

interface AuthorizeResponse {
  authCode: string;
}

interface TokenRequest {
  authCode: string;
  codeVerifier: string;
}

interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}

// Authentication API functions
export const authApi = {
  // Login function with PKCE flow
  async login(credentials: { email: string; password: string }): Promise<TokenResponse> {
    try {
      // Generate code verifier and challenge
      const codeVerifier = generateCodeVerifier();
      console.log(`Code verifier: ${codeVerifier}`);
      const codeChallenge = await generateCodeChallenge(codeVerifier);
      console.log(`Code challenge: ${codeChallenge}`);

      // Store code verifier in session storage for the token request
      sessionStorage.setItem('codeVerifier', codeVerifier);

      // Step 1: Get authorization code
      const authorizeResponse = await fetch(`${API_BASE_URL}/api/Auth/authorize`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: credentials.email,
          password: credentials.password,
          codeChallenge: codeChallenge,
        } as AuthorizeRequest),
      });

      //TODO: Fix this
      // const { authCode } = await handleResponse<AuthorizeResponse>(authorizeResponse);
      const authCode = await authorizeResponse.text();

      console.log(authCode);

      // Step 2: Exchange code for tokens
      const tokenResponse = await fetch(`${API_BASE_URL}/api/Auth/token`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          authCode: authCode,
          codeVerifier: codeVerifier,
        } as TokenRequest),
      });

      const tokens = await handleResponse<TokenResponse>(tokenResponse);

      // Store tokens
      localStorage.setItem('accessToken', tokens.accessToken);
      localStorage.setItem('refreshToken', tokens.refreshToken);

      return tokens;
    } catch (error: unknown) {
      console.error('Login error:', error);
      // Rethrow to be handled by the component
      throw error;
    }
  },

  // Register function
  async register(user: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/Auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(user),
    });

    return handleResponse<void>(response);
  },

  // Logout function
  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpiry');
    sessionStorage.removeItem('codeVerifier');
  }
};

// Protected API request helper
export const apiRequest = async <T>(
  url: string,
  options: RequestInit = {}
): Promise<T> => {
  const accessToken = localStorage.getItem('accessToken');

  if (!accessToken) {
    throw new Error('No access token found');
  }

  const headers = {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
    ...options.headers,
  };

  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    headers,
  });

  return handleResponse<T>(response);
};
