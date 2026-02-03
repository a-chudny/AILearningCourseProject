import { createContext, useState, useEffect, useCallback, ReactNode } from 'react';
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
   */
  const login = useCallback(async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await authService.login(email, password);
      setToken(response.token);
      setUser(response.user);
      // Navigate to home page after successful login
      navigate('/');
    } catch (error) {
      // Re-throw error to be handled by the component
      throw error;
    } finally {
      setIsLoading(false);
    }
  }, [navigate]);

  /**
   * Register function
   */
  const register = useCallback(async (data: RegisterRequest) => {
    setIsLoading(true);
    try {
      const response = await authService.register(data);
      setToken(response.token);
      setUser(response.user);
      // Navigate to home page after successful registration
      navigate('/');
    } catch (error) {
      // Re-throw error to be handled by the component
      throw error;
    } finally {
      setIsLoading(false);
    }
  }, [navigate]);

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
