import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import MyEventsPage from '@/pages/user/MyEventsPage';
import * as registrationService from '@/services/registrationService';
import { EventStatus } from '@/types/enums';
import type { RegistrationResponse } from '@/services/registrationService';

// Mock the registration service
vi.mock('@/services/registrationService');

const mockUpcomingRegistration: RegistrationResponse = {
  id: 1,
  eventId: 10,
  userId: 5,
  status: 'Confirmed',
  registeredAt: '2026-02-01T10:00:00Z',
  event: {
    id: 10,
    title: 'Beach Cleanup',
    location: 'Sunny Beach',
    startTime: '2026-03-15T09:00:00Z',
    durationMinutes: 180,
    status: EventStatus.Active,
    imageUrl: null,
  },
};

const mockPastRegistration: RegistrationResponse = {
  id: 2,
  eventId: 11,
  userId: 5,
  status: 'Confirmed',
  registeredAt: '2026-01-15T10:00:00Z',
  event: {
    id: 11,
    title: 'Food Bank Volunteering',
    location: 'Community Center',
    startTime: '2026-01-20T14:00:00Z',
    durationMinutes: 120,
    status: EventStatus.Active,
    imageUrl: null,
  },
};

const mockCancelledRegistration: RegistrationResponse = {
  id: 3,
  eventId: 12,
  userId: 5,
  status: 'Cancelled',
  registeredAt: '2026-01-10T10:00:00Z',
  event: {
    id: 12,
    title: 'Park Cleanup',
    location: 'Central Park',
    startTime: '2026-01-25T10:00:00Z',
    durationMinutes: 90,
    status: EventStatus.Active,
    imageUrl: null,
  },
};

const renderWithProviders = (component: React.ReactElement) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{component}</BrowserRouter>
    </QueryClientProvider>
  );
};

describe('MyEventsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows loading state while fetching registrations', () => {
    vi.mocked(registrationService.getMyRegistrations).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    renderWithProviders(<MyEventsPage />);

    expect(screen.getByText('Loading your events...')).toBeInTheDocument();
  });

  it('renders empty state when no registrations', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue([]);

    renderWithProviders(<MyEventsPage />);

    await waitFor(() => {
      expect(screen.getByText('No events yet')).toBeInTheDocument();
      expect(
        screen.getByText(/You haven't registered for any events/)
      ).toBeInTheDocument();
    });
  });

  it('displays error state on fetch failure', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockRejectedValue(
      new Error('Failed to fetch')
    );

    renderWithProviders(<MyEventsPage />);

    await waitFor(() => {
      expect(screen.getByText('Failed to load registrations')).toBeInTheDocument();
      expect(screen.getByText('Failed to fetch')).toBeInTheDocument();
    });
  });

  it('separates upcoming, past, and cancelled registrations into sections', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue([
      mockUpcomingRegistration,
      mockPastRegistration,
      mockCancelledRegistration,
    ]);

    renderWithProviders(<MyEventsPage />);

    await waitFor(() => {
      expect(screen.getByText(/Upcoming Events/)).toBeInTheDocument();
      expect(screen.getByText(/Past Events/)).toBeInTheDocument();
      expect(screen.getByText(/Cancelled Registrations/)).toBeInTheDocument();
    });
  });

  it('shows cancel button for upcoming events only', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue([
      mockUpcomingRegistration,
      mockPastRegistration,
    ]);

    renderWithProviders(<MyEventsPage />);

    await waitFor(() => {
      const cancelButtons = screen.getAllByRole('button', { name: /cancel/i });
      // Only upcoming event should have cancel button
      expect(cancelButtons).toHaveLength(1);
    });
  });

  it('opens cancel modal when cancel button is clicked', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue([
      mockUpcomingRegistration,
    ]);

    renderWithProviders(<MyEventsPage />);

    await waitFor(() => {
      expect(screen.getByText('Beach Cleanup')).toBeInTheDocument();
    });

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    fireEvent.click(cancelButton);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Cancel Registration' })).toBeInTheDocument();
    });
  });

  it('displays correct counts in section headers', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue([
      mockUpcomingRegistration,
      mockPastRegistration,
      mockCancelledRegistration,
    ]);

    renderWithProviders(<MyEventsPage />);

    await waitFor(() => {
      expect(screen.getByText('Upcoming Events')).toBeInTheDocument();
      // Check for section counts - there are 3 sections each with (1)
      const countElements = screen.getAllByText('(1)');
      expect(countElements).toHaveLength(3);
      expect(screen.getByText('Past Events')).toBeInTheDocument();
      expect(screen.getByText('Cancelled Registrations')).toBeInTheDocument();
    });
  });

  it('renders page title and description', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue([]);

    renderWithProviders(<MyEventsPage />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'My Events', level: 1 })).toBeInTheDocument();
    });
  });

  it('shows browse events link when user has registrations', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue([
      mockUpcomingRegistration,
    ]);

    renderWithProviders(<MyEventsPage />);

    await waitFor(() => {
      expect(screen.getByText('Browse more events')).toBeInTheDocument();
    });
  });

  it('does not show browse link in empty state', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue([]);

    renderWithProviders(<MyEventsPage />);

    await waitFor(() => {
      expect(screen.queryByText('Browse more events')).not.toBeInTheDocument();
    });
  });
});
