import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { UserRole } from '@/types/enums'
import { toast } from '@/utils/toast'

interface RoleGuardProps {
  children: ReactNode
  allowedRoles: UserRole[]
}

/**
 * Role guard component that checks if user has required role
 * Redirects unauthorized users to home page with toast notification
 */
export function RoleGuard({ children, allowedRoles }: RoleGuardProps) {
  const { user, isLoading } = useAuth()

  // Show loading state while checking authentication
  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
          <p className="text-gray-600">Loading...</p>
        </div>
      </div>
    )
  }

  // Check if user has any of the allowed roles (ANY logic)
  const hasRequiredRole = user && allowedRoles.includes(user.role)

  if (!hasRequiredRole) {
    // Show toast notification for forbidden access
    toast.error('You do not have permission to access this page')
    return <Navigate to="/" replace />
  }

  // User has required role, render the protected content
  return <>{children}</>
}
