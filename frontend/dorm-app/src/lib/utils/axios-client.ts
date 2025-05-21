import axios, { AxiosError, AxiosInstance, AxiosResponse, InternalAxiosRequestConfig } from "axios";
import { authService } from "../services/authService";

// Define base API URL from environment variable
const VITE_API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL ?? "http://localhost:5000";

// Create axios instance with default config
export const axiosClient: AxiosInstance = axios.create({
  baseURL: `${VITE_API_GATEWAY_URL}/api`,
  headers: {
    "Content-Type": "application/json",
  },
  // You can add more default configurations here
  timeout: 10000, // 10 seconds
});

// Extend the AxiosRequestConfig type to include _retry property
declare module 'axios' {
  export interface InternalAxiosRequestConfig {
    _retry?: boolean;
  }
}

// Store pending requests that should be retried after token refresh
let pendingRequests: Array<{
  resolve: (value: unknown) => void;
  reject: (reason?: unknown) => void;
  config: InternalAxiosRequestConfig;
}> = [];

// Request interceptor
axiosClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Get the token from localStorage
    const token = localStorage.getItem("accessToken");

    // If token exists, add it to the headers
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor
axiosClient.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig | undefined;
    
    if (!originalRequest) {
      return Promise.reject(error);
    }

    // Handle 401 Unauthorized - Try to refresh token
    if (
      error.response?.status === 401 &&
      localStorage.getItem("refreshToken") &&
      !originalRequest._retry
    ) {
      originalRequest._retry = true; // Mark this request as retried

      try {
        // Use authService's refreshToken method which handles concurrent refresh requests
        const refreshed = await authService.refreshToken();
          
        if (refreshed) {
          // Process all pending requests
          pendingRequests.forEach(request => {
            request.resolve(axiosClient(request.config));
          });
          pendingRequests = [];
            
          // Retry the original request with new token
          originalRequest.headers.Authorization = `Bearer ${localStorage.getItem("accessToken")}`;
          return axiosClient(originalRequest);
        } else {
          // Refresh failed, reject all pending requests
          pendingRequests.forEach(request => {
            request.reject(new Error("Token refresh failed"));
          });
          pendingRequests = [];
            
          // Redirect to login
          window.location.href = "/login";
          return Promise.reject(error);
        }
      } catch (refreshError) {
        pendingRequests = [];
        window.location.href = "/login";
        return Promise.reject(refreshError);
      }
    }

    // Handle 403 Forbidden errors
    if (error.response?.status === 403) {
      // Redirect to forbidden page or show error message
      window.location.href = "/forbidden";
      return Promise.reject(error);
    }

    // Handle network errors
    if (error.message === "Network Error") {
      // You can implement custom network error handling here
      console.error("Network error occurred");
      return Promise.reject(new Error("Network error occurred. Please check your connection."));
    }

    // Handle timeout errors
    if (error.code === "ECONNABORTED") {
      console.error("Request timeout");
      return Promise.reject(new Error("Request timeout. Please try again."));
    }

    // Handle other errors
    return Promise.reject(error);
  }
);

// Type for API error response
export interface ApiError {
  code: string;
  description: string;
  extensions?: Record<string, unknown>;
}

// Custom error class for API errors
export class ApiRequestError extends Error {
  constructor(
    public statusCode: number,
    public error: ApiError
  ) {
    super(error.description);
    this.name = "ApiRequestError";
  }
}

// Helper function to handle API errors
export function handleApiError(error: unknown): never {
  if (axios.isAxiosError(error) && error.response) {
    throw new ApiRequestError(
      error.response.status,
      error.response.data as ApiError
    );
  }
  throw error;
}

// Helper function to extract response data
export function extractResponseData<T>(response: AxiosResponse<T>): T {
  return response.data;
}

// Export reusable request methods with proper typing
export const api = {
  get: async <T>(url: string, params?: Record<string, unknown>) => {
    try {
      const response = await axiosClient.get<T>(url, { params });
      return extractResponseData(response);
    } catch (error) {
      throw handleApiError(error);
    }
  },

  getBlob: async (url: string, params?: Record<string, unknown>) => {
    try {
      const response = await axiosClient.get(url, { 
        params,
        responseType: 'blob' 
      });
      return response.data as Blob;
    } catch (error) {
      throw handleApiError(error);
    }
  },

  post: async <T>(url: string, data?: unknown) => {
    try {
      const response = await axiosClient.post<T>(url, data);
      return extractResponseData(response);
    } catch (error) {
      throw handleApiError(error);
    }
  },

  put: async <T>(url: string, data: unknown) => {
    try {
      const response = await axiosClient.put<T>(url, data);
      return extractResponseData(response);
    } catch (error) {
      throw handleApiError(error);
    }
  },

  patch: async <T>(url: string, data: unknown) => {
    try {
      const response = await axiosClient.patch<T>(url, data);
      return extractResponseData(response);
    } catch (error) {
      throw handleApiError(error);
    }
  },

  delete: async <T>(url: string) => {
    try {
      const response = await axiosClient.delete<T>(url);
      return extractResponseData(response);
    } catch (error) {
      throw handleApiError(error);
    }
  },
};

export default api;

