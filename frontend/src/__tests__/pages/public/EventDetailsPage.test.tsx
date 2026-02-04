import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import EventDetailsPage from '@/pages/public/EventDetailsPage';
import * as eventService from '@/services/eventService';
import * as registrationService from '@/services/registrationService';
import { AuthContext } from '@/context/AuthContext';
import { EventStatus, UserRole } from '@/types/enums';
import type { EventResponse } from '@/services/eventService';
import type { User } from '@/types';

// Mock services
vi.mock('@/services/eventService');
vi.mock('@/services/registrationService');
vi.mock('@/utils/toast', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

const mockEvent: EventResponse = {
  id: 1,
  title: 'Beach Cleanup Event',
  description: 'Help clean up the local beach and make a difference in our community.',
  location: '123 Beach Road, Coastal City',
  startTime: '2026-03-15T10:00:00Z',
  durationMinutes: 180,
  capacity: 50,
  imageUrl: 'https://example.com/beach.jpg',
  registrationDeadline: '2026-03-14T23:59:59Z',
  status: EventStatus.Active,
  organizerId: 2,
  organizerName: 'John Organizer',
  registrationCount: 25,
  requiredSkills: [
    { id: 1, name: 'First Aid', description: 'Basic first aid and CPR skills', createdAt: '2026-01-01T00:00:00Z' },
    { id: 2, name: 'Driving', description: 'Valid driver license', createdAt: '2026-01-01T00:00:00Z' },
  ],
  createdAt: '2026-02-01T00:00:00Z',
  updatedAt: '2026-02-01T00:00:00Z',
};

const mockVolunteerUser: User = {
  id: 1,
  email: 'volunteer@test.com',
  name: 'Test Volunteer',
  role: UserRole.Volunteer,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
};

const mockOrganizerUser: User = {
  id: 2,
  email: 'organizer@test.com',
  name: 'Test Organizer',
  role: UserRole.Organizer,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
};

const renderWithProviders = (
  component: React.ReactElement,
  { user = null, isAuthenticated = false, isLoading = false } = {} as {
    user?: User | null;
    isAuthenticated?: boolean;
    isLoading?: boolean;
  }
) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  const mockAuthContextValue = {
    user,
    token: null,
    isAuthenticated,
    isLoading,
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    refetchUser: vi.fn(),
  };

  return render(
    <QueryClientProvider client={queryClient}>
      <AuthContext.Provider value={mockAuthContextValue}>
        <BrowserRouter>
          <Routes>
            <Route path="/events/:id" element={component} />
          </Routes>
        </BrowserRouter>
      </AuthContext.Provider>
    </QueryClientProvider>,
    { wrapper: ({ children }) => <div>{children}</div> }
  );
};

describe('EventDetailsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Mock registration check to return not registered by default
    vi.mocked(registrationService.checkUserRegistration).mockResolvedValue({
      isRegistered: false,
      registration: undefined,
    });
  });

  it('displays loading state while fetching event', () => {
    vi.mocked(eventService.getEventById).mockImplementation(() => new Promise(() => {})); // Never resolves

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    expect(screen.getByText('Loading event details...')).toBeInTheDocument();
  });

  it('displays error state when event is not found', async () => {
    vi.mocked(eventService.getEventById).mockRejectedValue(new Error('Event not found'));

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Event not found' })).toBeInTheDocument();
    });

    expect(screen.getByText('Back to Events')).toBeInTheDocument();
  });

  it('displays event details correctly', async () => {
    vi.mocked(eventService.getEventById).mockResolvedValue(mockEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    await waitFor(() => {
      expect(screen.getByText('Beach Cleanup Event')).toBeInTheDocument();
    });

    expect(screen.getByText('Organized by John Organizer')).toBeInTheDocument();
    expect(screen.getByText(/123 Beach Road, Coastal City/)).toBeInTheDocument();
    expect(screen.getByText('25 / 50 registered')).toBeInTheDocument();
    expect(screen.getByText(/25 spots left/)).toBeInTheDocument();
  });

  it('displays event image when provided', async () => {
    vi.mocked(eventService.getEventById).mockResolvedValue(mockEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    await waitFor(() => {
      const image = screen.getByAltText('Beach Cleanup Event');
      expect(image).toHaveAttribute('src', 'https://example.com/beach.jpg');
    });
  });

  it('displays required skills with hover descriptions', async () => {
    vi.mocked(eventService.getEventById).mockResolvedValue(mockEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    await waitFor(() => {
      expect(screen.getByText('Required Skills')).toBeInTheDocument();
    });

    expect(screen.getByText('First Aid')).toBeInTheDocument();
    expect(screen.getByText('Driving')).toBeInTheDocument();
  });

  it('shows login prompt for unauthenticated users', async () => {
    vi.mocked(eventService.getEventById).mockResolvedValue(mockEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    await waitFor(() => {
      expect(screen.getByText(/Log in/)).toBeInTheDocument();
    });

    expect(screen.getByText(/create an account/)).toBeInTheDocument();
    expect(screen.getByText(/to register for this event/)).toBeInTheDocument();
  });

  it('shows register button for authenticated volunteer', async () => {
    vi.mocked(eventService.getEventById).mockResolvedValue(mockEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />, {
      user: mockVolunteerUser,
      isAuthenticated: true,
    });

    await waitFor(() => {
      expect(screen.getByText('Register for Event')).toBeInTheDocument();
    });
  });

  it('shows edit and cancel event buttons for event owner', async () => {
    vi.mocked(eventService.getEventById).mockResolvedValue(mockEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />, {
      user: mockOrganizerUser,
      isAuthenticated: true,
    });

    await waitFor(() => {
      expect(screen.getByText('Edit Event')).toBeInTheDocument();
    });

    expect(screen.getByText('Cancel Event')).toBeInTheDocument();
  });

  it('shows cancelled badge when event is cancelled', async () => {
    const cancelledEvent = { ...mockEvent, status: EventStatus.Cancelled };
    vi.mocked(eventService.getEventById).mockResolvedValue(cancelledEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    await waitFor(() => {
      expect(screen.getByText('Event Cancelled')).toBeInTheDocument();
    });
  });

  it('shows full indicator when event is at capacity', async () => {
    const fullEvent = { ...mockEvent, registrationCount: 50 };
    vi.mocked(eventService.getEventById).mockResolvedValue(fullEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    await waitFor(() => {
      expect(screen.getByText('(Full)')).toBeInTheDocument();
    });
  });

  it('shows registration closed message when deadline has passed', async () => {
    const pastDeadlineEvent = {
      ...mockEvent,
      registrationDeadline: '2020-01-01T00:00:00Z',
    };
    vi.mocked(eventService.getEventById).mockResolvedValue(pastDeadlineEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />, {
      user: mockVolunteerUser,
      isAuthenticated: true,
    });

    await waitFor(() => {
      expect(screen.getByText(/Registration Closed/)).toBeInTheDocument();
    });
  });

  it('displays Google Maps link for location', async () => {
    vi.mocked(eventService.getEventById).mockResolvedValue(mockEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    await waitFor(() => {
      const locationLink = screen.getByText('123 Beach Road, Coastal City');
      expect(locationLink).toHaveAttribute(
        'href',
        expect.stringContaining('google.com/maps')
      );
      expect(locationLink).toHaveAttribute('target', '_blank');
    });
  });

  it('formats duration correctly', async () => {
    vi.mocked(eventService.getEventById).mockResolvedValue(mockEvent);

    window.history.pushState({}, '', '/events/1');
    renderWithProviders(<EventDetailsPage />);

    await waitFor(() => {
      expect(screen.getByText('3 hours')).toBeInTheDocument();
    });
  });
});
