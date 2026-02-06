import { render, screen } from '@testing-library/react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { AuthContext } from '@/context/AuthContext';
import type { AuthContextType } from '@/context/AuthContext';

// Test component to render inside protected route
function ProtectedContent() {
  return <div>Protected Content</div>;
}

function LoginPage() {
  return <div>Login Page</div>;
}

describe('ProtectedRoute', () => {
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
  });

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows loading state while checking authentication', () => {
    const authContext = mockAuthContext({ isLoading: true });

    render(
      <AuthContext value={authContext}>
        <BrowserRouter>
          <ProtectedRoute>
            <ProtectedContent />
          </ProtectedRoute>
        </BrowserRouter>
      </AuthContext>
    );

    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('redirects to login when user is not authenticated', () => {
    const authContext = mockAuthContext({ isAuthenticated: false, isLoading: false });

    render(
      <AuthContext value={authContext}>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <ProtectedContent />
                </ProtectedRoute>
              }
            />
          </Routes>
        </BrowserRouter>
      </AuthContext>
    );

    expect(screen.getByText('Login Page')).toBeInTheDocument();
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('renders protected content when user is authenticated', () => {
    const authContext = mockAuthContext({
      isAuthenticated: true,
      isLoading: false,
      user: {
        id: 1,
        email: 'test@test.com',
        name: 'Test User',
        role: 0,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      },
    });

    render(
      <AuthContext value={authContext}>
        <BrowserRouter>
          <ProtectedRoute>
            <ProtectedContent />
          </ProtectedRoute>
        </BrowserRouter>
      </AuthContext>
    );

    expect(screen.getByText('Protected Content')).toBeInTheDocument();
    expect(screen.queryByText('Loading...')).not.toBeInTheDocument();
  });
});
