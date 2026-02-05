import axios, { AxiosError } from 'axios'
import type { AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import toast from 'react-hot-toast'

// API base URL - uses Vite proxy in development
const API_BASE_URL = '/api'

// Flag to track intentional logout to suppress error toasts
let isLoggingOut = false;
// Track when logout started to suppress errors for a grace period
let logoutTimestamp = 0;

export function setLoggingOut(value: boolean) {
  isLoggingOut = value;
  if (value) {
    logoutTimestamp = Date.now();
  } else {
    // Reset timestamp when explicitly setting to false
    logoutTimestamp = 0;
  }
}

// Check if we're in the logout grace period (within 3 seconds of logout)
export function isInLogoutGracePeriod(): boolean {
  if (isLoggingOut) return true;
  // Also suppress toasts for 3 seconds after logout flag was set
  if (logoutTimestamp > 0 && (Date.now() - logoutTimestamp) < 3000) return true;
  // Otherwise, not in logout grace period
  return false;
}

// Create axios instance with default configuration
export const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor for adding auth token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('auth_token')
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error: AxiosError) => {
    return Promise.reject(error)
  }
)

// Response interceptor for handling errors
api.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    // Handle network errors (no response)
    if (!error.response) {
      if (error.code === 'ECONNABORTED' || error.message.includes('timeout')) {
        toast.error('Request timed out. Please check your connection and try again.')
      } else if (error.message === 'Network Error') {
        toast.error('Unable to connect to server. Please check your internet connection.')
      } else {
        toast.error('A network error occurred. Please try again.')
      }
      return Promise.reject(error)
    }

    const { status } = error.response

    // Handle specific status codes
    if (status === 401) {
      // Skip toast if logging out intentionally or in grace period
      if (isInLogoutGracePeriod()) {
        return Promise.reject(error)
      }
      // Check if this was an intentional logout (token already removed)
      const hasToken = localStorage.getItem('auth_token')
      if (hasToken) {
        // Token is invalid or expired - clean up and redirect
        localStorage.removeItem('auth_token')
        
        // Only redirect and show toast if not already on login page
        if (!window.location.pathname.includes('/login')) {
          toast.error('Your session has expired. Please log in again.')
          window.location.href = '/login'
        }
      }
      // If no token, this is likely a request made during/after logout - silently ignore
    } else if (status === 403) {
      // Skip toast if logging out intentionally or in grace period
      if (isInLogoutGracePeriod()) {
        return Promise.reject(error)
      }
      // Only show permission error if user is actually logged in
      const hasToken = localStorage.getItem('auth_token')
      if (hasToken) {
        toast.error('You do not have permission to perform this action.')
      }
    } else if (status === 404) {
      // Don't show toast for 404 - let components handle it
    } else if (status >= 500) {
      toast.error('Server error. Please try again later.')
    }
    // Don't show toast for 400 validation errors - let forms handle them

    return Promise.reject(error)
  }
)

// Type-safe API response wrapper
export interface ApiResponse<T> {
  data: T
  message?: string
}

// Type-safe API error response (matches backend ApiErrorResponse)
export interface ApiErrorResponse {
  message: string
  code: string
  statusCode: number
  errors?: Record<string, string[]>
  traceId?: string
  timestamp?: string
}

/**
 * Helper function to extract error message from API response
 */
export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const apiError = error.response?.data as ApiErrorResponse | undefined
    return apiError?.message ?? error.message ?? 'An unexpected error occurred'
  }
  if (error instanceof Error) {
    return error.message
  }
  return 'An unexpected error occurred'
}

/**
 * Helper function to extract validation errors from API response
 */
export function getValidationErrors(error: unknown): Record<string, string[]> | null {
  if (axios.isAxiosError(error)) {
    const apiError = error.response?.data as ApiErrorResponse | undefined
    return apiError?.errors ?? null
  }
  return null
}

/**
 * Helper function to check if error is a specific status code
 */
export function isErrorStatus(error: unknown, status: number): boolean {
  if (axios.isAxiosError(error)) {
    return error.response?.status === status
  }
  return false
}

export default api
