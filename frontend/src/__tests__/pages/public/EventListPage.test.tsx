import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import EventListPage from '@/pages/public/EventListPage';
import * as eventService from '@/services/eventService';
import { EventStatus } from '@/types/enums';
import type { EventListResponse } from '@/services/eventService';

// Mock the event service
vi.mock('@/services/eventService');

const mockEventListResponse: EventListResponse = {
  events: [
    {
      id: 1,
      title: 'Beach Cleanup',
      description: 'Help clean the beach',
      location: 'Sunny Beach',
      startTime: '2026-03-15T10:00:00Z',
      durationMinutes: 180,
      capacity: 50,
      registrationCount: 25,
      status: EventStatus.Active,
      organizerId: 2,
      organizerName: 'John Doe',
      requiredSkills: [
        {
          id: 1,
          name: 'First Aid',
          category: 'Medical',
          createdAt: '2026-01-01T00:00:00Z',
        },
      ],
      createdAt: '2026-02-01T00:00:00Z',
      updatedAt: '2026-02-01T00:00:00Z',
    },
    {
      id: 2,
      title: 'Food Bank Volunteering',
      description: 'Sort and pack food donations',
      location: 'Community Center',
      startTime: '2026-03-20T14:00:00Z',
      durationMinutes: 120,
      capacity: 30,
      registrationCount: 15,
      status: EventStatus.Active,
      organizerId: 3,
      organizerName: 'Jane Smith',
      requiredSkills: [],
      createdAt: '2026-02-02T00:00:00Z',
      updatedAt: '2026-02-02T00:00:00Z',
    },
  ],
  page: 1,
  pageSize: 20,
  totalCount: 2,
  totalPages: 1,
  hasPreviousPage: false,
  hasNextPage: false,
};

const emptyResponse: EventListResponse = {
  events: [],
  page: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

// Helper to render with providers
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

describe('EventListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders page header and description', () => {
    vi.mocked(eventService.getEvents).mockResolvedValue(mockEventListResponse);

    renderWithProviders(<EventListPage />);

    expect(screen.getByText('Volunteer Events')).toBeInTheDocument();
    expect(
      screen.getByText('Discover opportunities to make a difference in your community')
    ).toBeInTheDocument();
  });

  it('displays loading state while fetching events', () => {
    vi.mocked(eventService.getEvents).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    const { container } = renderWithProviders(<EventListPage />);

    // POL-002: Loading state now uses skeleton loaders instead of text
    const skeletons = container.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('displays events when data is loaded', async () => {
    vi.mocked(eventService.getEvents).mockResolvedValue(mockEventListResponse);

    renderWithProviders(<EventListPage />);

    await waitFor(() => {
      expect(screen.getByText('Beach Cleanup')).toBeInTheDocument();
    });

    expect(screen.getByText('Food Bank Volunteering')).toBeInTheDocument();
  });

  it('displays empty state when no events found', async () => {
    vi.mocked(eventService.getEvents).mockResolvedValue(emptyResponse);

    renderWithProviders(<EventListPage />);

    await waitFor(() => {
      expect(screen.getByText('No events found')).toBeInTheDocument();
    });

    expect(
      screen.getByText('Check back later for upcoming volunteer opportunities')
    ).toBeInTheDocument();
  });

  it('displays error state when fetch fails', async () => {
    vi.mocked(eventService.getEvents).mockRejectedValue(new Error('Network error'));

    renderWithProviders(<EventListPage />);

    await waitFor(() => {
      expect(screen.getByText('Failed to load events')).toBeInTheDocument();
    });

    expect(screen.getByText('Network error')).toBeInTheDocument();
  });

  it('renders filters component', () => {
    vi.mocked(eventService.getEvents).mockResolvedValue(mockEventListResponse);

    renderWithProviders(<EventListPage />);

    expect(screen.getByLabelText('Search Events')).toBeInTheDocument();
    expect(screen.getByLabelText('Event Status')).toBeInTheDocument();
    expect(screen.getByLabelText('Include past events')).toBeInTheDocument();
  });

  it('renders page size selector with correct default', () => {
    vi.mocked(eventService.getEvents).mockResolvedValue(mockEventListResponse);

    renderWithProviders(<EventListPage />);

    const pageSizeSelect = screen.getByLabelText('Events per page:') as HTMLSelectElement;
    expect(pageSizeSelect).toBeInTheDocument();
    expect(pageSizeSelect.value).toBe('20');
  });

  it('displays results count correctly', async () => {
    vi.mocked(eventService.getEvents).mockResolvedValue(mockEventListResponse);

    renderWithProviders(<EventListPage />);

    await waitFor(() => {
      expect(screen.getByText(/Showing 1 - 2 of 2 events/)).toBeInTheDocument();
    });
  });

  it('renders event cards in grid layout', async () => {
    vi.mocked(eventService.getEvents).mockResolvedValue(mockEventListResponse);

    const { container } = renderWithProviders(<EventListPage />);

    await waitFor(() => {
      // Find the events grid specifically (last grid on the page after filters grid)
      const grids = container.querySelectorAll('.grid');
      const eventsGrid = grids[grids.length - 1]; // Last grid is the events grid
      expect(eventsGrid).toBeInTheDocument();
      expect(eventsGrid?.children).toHaveLength(2);
    });
  });
});
