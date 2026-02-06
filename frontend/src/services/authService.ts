import { api, setLoggingOut } from './api';
import type { LoginRequest, RegisterRequest, AuthResponse, User } from '@/types';

const AUTH_TOKEN_KEY = 'auth_token';

/**
 * Authentication service for handling user authentication operations
 */
export const authService = {
  /**
   * Login with email and password
   */
  async login(email: string, password: string): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/login', {
      email,
      password,
    } as LoginRequest);
    
    // Store token in localStorage
    if (response.data.token) {
      localStorage.setItem(AUTH_TOKEN_KEY, response.data.token);
    }
    
    return response.data;
  },

  /**
   * Register a new user account
   */
  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/register', data);
    
    // Store token in localStorage
    if (response.data.token) {
      localStorage.setItem(AUTH_TOKEN_KEY, response.data.token);
    }
    
    return response.data;
  },

  /**
   * Get current authenticated user information
   */
  async getCurrentUser(): Promise<User> {
    const response = await api.get<User>('/auth/me');
    return response.data;
  },

  /**
   * Logout the current user
   */
  logout(): void {
    // Set flag to suppress error toasts during logout
    setLoggingOut(true);
    localStorage.removeItem(AUTH_TOKEN_KEY);
    // Reset flag after 4 seconds (longer than the 3s grace period)
    setTimeout(() => setLoggingOut(false), 4000);
  },

  /**
   * Get stored auth token
   */
  getToken(): string | null {
    return localStorage.getItem(AUTH_TOKEN_KEY);
  },

  /**
   * Check if user is authenticated (has valid token)
   */
  isAuthenticated(): boolean {
    return !!this.getToken();
  },
};
