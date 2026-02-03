import { useContext } from 'react';
import { AuthContext, AuthContextType } from '@/context/AuthContext';

/**
 * Custom hook to access authentication context
 * Must be used within an AuthProvider
 * 
 * @returns Authentication context with user, token, and auth functions
 * @throws Error if used outside AuthProvider
 * 
 * @example
 * ```tsx
 * function MyComponent() {
 *   const { user, isAuthenticated, login, logout } = useAuth();
 *   
 *   if (!isAuthenticated) {
 *     return <LoginButton onClick={() => login(email, password)} />;
 *   }
 *   
 *   return <div>Welcome, {user?.name}!</div>;
 * }
 * ```
 */
export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  
  return context;
}
