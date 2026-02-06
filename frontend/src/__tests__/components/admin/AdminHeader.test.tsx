import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { AdminHeader } from '@/components/admin/AdminHeader';
import { AuthContext } from '@/context/AuthContext';
import { UserRole } from '@/types/enums';

const mockUser = {
  id: 1,
  email: 'admin@test.com',
  name: 'Admin User',
  role: UserRole.Admin,
  phoneNumber: undefined,
  skills: [],
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
};

const mockAuthContextValue = {
  user: mockUser,
  token: 'mock-token',
  isAuthenticated: true,
  isLoading: false,
  login: vi.fn(),
  register: vi.fn(),
  logout: vi.fn(),
  refetchUser: vi.fn(),
};

describe('AdminHeader', () => {
  it('renders back to main site link', () => {
    render(
      <AuthContext value={mockAuthContextValue}>
        <BrowserRouter>
          <AdminHeader />
        </BrowserRouter>
      </AuthContext>
    );

    const backLink = screen.getByText('Back to Main Site');
    expect(backLink).toBeInTheDocument();
    expect(backLink.closest('a')).toHaveAttribute('href', '/');
  });

  it('displays user name and role', () => {
    render(
      <AuthContext value={mockAuthContextValue}>
        <BrowserRouter>
          <AdminHeader />
        </BrowserRouter>
      </AuthContext>
    );

    expect(screen.getByText('Admin User')).toBeInTheDocument();
    expect(screen.getByText('Admin')).toBeInTheDocument();
  });
});
