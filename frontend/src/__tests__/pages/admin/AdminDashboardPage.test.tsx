import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import AdminDashboardPage from '@/pages/admin/AdminDashboardPage';
import * as adminService from '@/services/adminService';

// Mock the admin service
vi.mock('@/services/adminService');

const mockStats = {
  totalUsers: 150,
  totalEvents: 45,
  totalRegistrations: 320,
  registrationsThisMonth: 28,
  upcomingEvents: 12,
};

function renderWithProviders(component: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{component}</BrowserRouter>
    </QueryClientProvider>
  );
}

describe('AdminDashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders dashboard title and description', async () => {
    vi.mocked(adminService.getAdminStats).mockResolvedValue(mockStats);

    renderWithProviders(<AdminDashboardPage />);

    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.getByText('Welcome to the admin dashboard')).toBeInTheDocument();
  });

  it('displays loading state initially', () => {
    vi.mocked(adminService.getAdminStats).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    renderWithProviders(<AdminDashboardPage />);

    // Check for loading skeleton
    const loadingSkeletons = screen.getAllByRole('generic').filter((el) =>
      el.className.includes('animate-pulse')
    );
    expect(loadingSkeletons.length).toBeGreaterThan(0);
  });

  it('displays statistics when data loads successfully', async () => {
    vi.mocked(adminService.getAdminStats).mockResolvedValue(mockStats);

    renderWithProviders(<AdminDashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('150')).toBeInTheDocument(); // Total Users
      expect(screen.getByText('45')).toBeInTheDocument(); // Total Events
      expect(screen.getByText('320')).toBeInTheDocument(); // Total Registrations
      expect(screen.getByText('28')).toBeInTheDocument(); // Registrations This Month
    });
  });

  it('displays stat card titles', async () => {
    vi.mocked(adminService.getAdminStats).mockResolvedValue(mockStats);

    renderWithProviders(<AdminDashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('Total Users')).toBeInTheDocument();
      expect(screen.getByText('Total Events')).toBeInTheDocument();
      expect(screen.getByText('Total Registrations')).toBeInTheDocument();
      expect(screen.getByText('Registrations This Month')).toBeInTheDocument();
    });
  });

  it('displays quick actions section', async () => {
    vi.mocked(adminService.getAdminStats).mockResolvedValue(mockStats);

    renderWithProviders(<AdminDashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('Quick Actions')).toBeInTheDocument();
      expect(screen.getByText('Manage Users')).toBeInTheDocument();
      expect(screen.getByText('Manage Events')).toBeInTheDocument();
      expect(screen.getByText('System Settings')).toBeInTheDocument();
    });
  });

  it('displays error message when stats fail to load', async () => {
    vi.mocked(adminService.getAdminStats).mockRejectedValue(
      new Error('Failed to fetch')
    );

    renderWithProviders(<AdminDashboardPage />);

    await waitFor(
      () => {
        expect(
          screen.getByText('Failed to load dashboard statistics. Please try again later.')
        ).toBeInTheDocument();
      },
      { timeout: 3000 }
    );
  });

  it('quick action links have correct hrefs', async () => {
    vi.mocked(adminService.getAdminStats).mockResolvedValue(mockStats);

    renderWithProviders(<AdminDashboardPage />);

    await waitFor(() => {
      const manageUsersLink = screen.getByText('Manage Users').closest('a');
      const manageEventsLink = screen.getByText('Manage Events').closest('a');
      const settingsLink = screen.getByText('System Settings').closest('a');

      expect(manageUsersLink).toHaveAttribute('href', '/admin/users');
      expect(manageEventsLink).toHaveAttribute('href', '/admin/events');
      expect(settingsLink).toHaveAttribute('href', '/admin/settings');
    });
  });
});
