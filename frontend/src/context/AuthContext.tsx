import { createContext, useState, useEffect, useCallback } from 'react';
import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '@/services/authService';
import type { User, RegisterRequest } from '@/types';

/**
 * Authentication context type definition
 */
export interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  refetchUser: () => Promise<void>;
}

/**
 * Authentication context
 */
// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextType | undefined>(undefined);

/**
 * Authentication provider props
 */
interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Authentication provider component
 * Manages authentication state and provides auth functions to the app
 */
export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  /**
   * Fetch current user from API
   */
  const fetchUser = useCallback(async () => {
    const storedToken = authService.getToken();

    if (!storedToken) {
      setIsLoading(false);
      return;
    }

    try {
      const userData = await authService.getCurrentUser();
      setUser(userData);
      setToken(storedToken);
    } catch (error) {
      // Token is invalid or expired
      console.error('Failed to fetch user:', error);
      authService.logout();
      setUser(null);
      setToken(null);
    } finally {
      setIsLoading(false);
    }
  }, []);

  /**
   * Initialize auth state on mount
   */
  useEffect(() => {
    fetchUser();
  }, [fetchUser]);

  /**
   * Login function
   * Note: Navigation is handled by the calling component to support return URLs
   */
  const login = useCallback(async (email: string, password: string) => {
    const response = await authService.login(email, password);
    // Set auth state - use a callback to ensure state is updated
    setToken(response.token);
    setUser(response.user);

    // Small delay to ensure React processes the state updates
    // This prevents navigation happening before state propagates
    await new Promise((resolve) => setTimeout(resolve, 0));
    // Note: No automatic navigation - let calling component handle redirect
  }, []);

  /**
   * Register function
   * Sets auth state and navigates to home page after successful registration
   * Uses window.location.href like login for reliable full page refresh
   */
  const register = useCallback(async (data: RegisterRequest) => {
    setIsLoading(true);
    try {
      const response = await authService.register(data);
      setToken(response.token);
      setUser(response.user);
      // Use window.location.href for full page reload like login does
      // This ensures all state is fresh and components re-render properly
      window.location.href = '/';
    } finally {
      setIsLoading(false);
    }
  }, []);

  /**
   * Logout function
   */
  const logout = useCallback(() => {
    authService.logout();
    setUser(null);
    setToken(null);
    // Navigate to login page after logout
    navigate('/login');
  }, [navigate]);

  /**
   * Refetch user data (useful after profile updates)
   */
  const refetchUser = useCallback(async () => {
    if (token) {
      try {
        const userData = await authService.getCurrentUser();
        setUser(userData);
      } catch (error) {
        console.error('Failed to refetch user:', error);
      }
    }
  }, [token]);

  const value: AuthContextType = {
    user,
    token,
    isAuthenticated: !!user && !!token,
    isLoading,
    login,
    register,
    logout,
    refetchUser,
  };

  return <AuthContext value={value}>{children}</AuthContext>;
}
