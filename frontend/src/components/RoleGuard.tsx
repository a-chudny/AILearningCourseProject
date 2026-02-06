import { type ReactNode, useRef } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { UserRole } from '@/types/enums';
import { toast } from '@/utils/toast';
import { isInLogoutGracePeriod } from '@/services/api';

interface RoleGuardProps {
  children: ReactNode;
  allowedRoles: UserRole[];
}

// Track last toast time globally to prevent duplicate toasts across multiple RoleGuard instances
let lastToastTime = 0;
const TOAST_COOLDOWN = 3000; // 3 seconds cooldown between toasts

/**
 * Role guard component that checks if user has required role
 * Redirects unauthorized users to home page with toast notification
 */
export function RoleGuard({ children, allowedRoles }: RoleGuardProps) {
  const { user, isLoading } = useAuth();
  const hasShownToast = useRef(false);

  // Show loading state while checking authentication
  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
          <p className="text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  // Check if user has any of the allowed roles (ANY logic)
  const hasRequiredRole = user && allowedRoles.includes(user.role);

  if (!hasRequiredRole) {
    // Show toast only once per cooldown period (prevents duplicate toasts from multiple RoleGuard instances)
    // But skip toast during logout process (uses the same flag as API error interceptor)
    const now = Date.now();
    if (
      !isInLogoutGracePeriod() &&
      !hasShownToast.current &&
      now - lastToastTime > TOAST_COOLDOWN
    ) {
      toast.error('You do not have permission to access this page');
      hasShownToast.current = true;
      lastToastTime = now;
    }
    return <Navigate to="/" replace />;
  }

  // User has required role, render the protected content
  return <>{children}</>;
}
