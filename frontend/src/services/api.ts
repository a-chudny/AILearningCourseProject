import axios, { AxiosError } from 'axios'
import type { AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import toast from 'react-hot-toast'

// API base URL - uses Vite proxy in development
const API_BASE_URL = '/api'

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
      // Handle unauthorized - token is invalid or expired
      localStorage.removeItem('auth_token')
      
      // Only redirect if not already on login page to avoid infinite loops
      if (!window.location.pathname.includes('/login')) {
        toast.error('Your session has expired. Please log in again.')
        window.location.href = '/login'
      }
    } else if (status === 403) {
      toast.error('You do not have permission to perform this action.')
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
