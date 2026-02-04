import { render, screen } from '@testing-library/react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { RoleGuard } from '@/components/RoleGuard'
import { AuthContext } from '@/context/AuthContext'
import type { AuthContextType } from '@/context/AuthContext'
import { UserRole } from '@/types/enums'

// Mock toast utility
vi.mock('@/utils/toast', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
    info: vi.fn(),
    warning: vi.fn(),
  },
}))

// Test component to render inside role guard
function AdminContent() {
  return <div>Admin Only Content</div>
}

function HomePage() {
  return <div>Home Page</div>
}

describe('RoleGuard', () => {
  const mockAuthContext = (overrides: Partial<AuthContextType> = {}): AuthContextType => ({
    user: null,
    token: null,
    isAuthenticated: false,
    isLoading: false,
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    refetchUser: vi.fn(),
    ...overrides,
  })

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows loading state while checking authentication', () => {
    const authContext = mockAuthContext({ isLoading: true })

    render(
      <AuthContext value={authContext}>
        <BrowserRouter>
          <RoleGuard allowedRoles={[UserRole.Admin]}>
            <AdminContent />
          </RoleGuard>
        </BrowserRouter>
      </AuthContext>
    )

    expect(screen.getByText('Loading...')).toBeInTheDocument()
    expect(screen.queryByText('Admin Only Content')).not.toBeInTheDocument()
  })

  it('redirects to home when user does not have required role', () => {
    const authContext = mockAuthContext({
      isAuthenticated: true,
      isLoading: false,
      user: { 
        id: 1, 
        email: 'volunteer@test.com', 
        name: 'Volunteer User', 
        role: UserRole.Volunteer, 
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
    })

    render(
      <AuthContext value={authContext}>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route
              path="/admin"
              element={
                <RoleGuard allowedRoles={[UserRole.Admin]}>
                  <AdminContent />
                </RoleGuard>
              }
            />
          </Routes>
        </BrowserRouter>
      </AuthContext>
    )

    expect(screen.getByText('Home Page')).toBeInTheDocument()
    expect(screen.queryByText('Admin Only Content')).not.toBeInTheDocument()
  })

  it('renders protected content when user has required role', () => {
    const authContext = mockAuthContext({
      isAuthenticated: true,
      isLoading: false,
      user: { 
        id: 1, 
        email: 'admin@test.com', 
        name: 'Admin User', 
        role: UserRole.Admin, 
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
    })

    render(
      <AuthContext value={authContext}>
        <BrowserRouter>
          <RoleGuard allowedRoles={[UserRole.Admin]}>
            <AdminContent />
          </RoleGuard>
        </BrowserRouter>
      </AuthContext>
    )

    expect(screen.getByText('Admin Only Content')).toBeInTheDocument()
    expect(screen.queryByText('Loading...')).not.toBeInTheDocument()
  })

  it('allows access when user has one of multiple allowed roles', () => {
    const authContext = mockAuthContext({
      isAuthenticated: true,
      isLoading: false,
      user: { 
        id: 1, 
        email: 'organizer@test.com', 
        name: 'Organizer User', 
        role: UserRole.Organizer, 
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
    })

    render(
      <AuthContext value={authContext}>
        <BrowserRouter>
          <RoleGuard allowedRoles={[UserRole.Organizer, UserRole.Admin]}>
            <AdminContent />
          </RoleGuard>
        </BrowserRouter>
      </AuthContext>
    )

    expect(screen.getByText('Admin Only Content')).toBeInTheDocument()
  })

  it('redirects when user is null', () => {
    const authContext = mockAuthContext({
      isAuthenticated: false,
      isLoading: false,
      user: null,
    })

    render(
      <AuthContext value={authContext}>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route
              path="/admin"
              element={
                <RoleGuard allowedRoles={[UserRole.Admin]}>
                  <AdminContent />
                </RoleGuard>
              }
            />
          </Routes>
        </BrowserRouter>
      </AuthContext>
    )

    expect(screen.getByText('Home Page')).toBeInTheDocument()
    expect(screen.queryByText('Admin Only Content')).not.toBeInTheDocument()
  })
})
