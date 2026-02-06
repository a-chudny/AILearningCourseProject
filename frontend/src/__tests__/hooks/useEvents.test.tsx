import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import {
  useEvents,
  useEvent,
  useCreateEvent,
  useUpdateEvent,
  useDeleteEvent,
  useCancelEvent,
  eventKeys,
} from '@/hooks/useEvents';
import * as eventService from '@/services/eventService';
import type { EventResponse, EventListResponse } from '@/services/eventService';

// Mock the event service
vi.mock('@/services/eventService');

const mockEvent: EventResponse = {
  id: 1,
  title: 'Beach Cleanup',
  description: 'Help clean up the beach',
  location: 'Santa Monica',
  startTime: '2024-03-01T10:00:00',
  durationMinutes: 120,
  capacity: 50,
  registeredCount: 10,
  status: 'Upcoming',
  imageUrl: null,
  registrationDeadline: null,
  createdAt: '2024-02-01T00:00:00',
  organizer: { id: 1, name: 'John Doe', email: 'john@example.com' },
  requiredSkills: [],
};

const mockEventList: EventListResponse = {
  items: [mockEvent],
  totalCount: 1,
  page: 1,
  pageSize: 10,
  totalPages: 1,
};

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe('eventKeys', () => {
  it('creates correct all key', () => {
    expect(eventKeys.all).toEqual(['events']);
  });

  it('creates correct lists key', () => {
    expect(eventKeys.lists()).toEqual(['events', 'list']);
  });

  it('creates correct list key with params', () => {
    const params = { page: 1, pageSize: 10 };
    expect(eventKeys.list(params)).toEqual(['events', 'list', params]);
  });

  it('creates correct details key', () => {
    expect(eventKeys.details()).toEqual(['events', 'detail']);
  });

  it('creates correct detail key with id', () => {
    expect(eventKeys.detail(1)).toEqual(['events', 'detail', 1]);
  });
});

describe('useEvents', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches events successfully', async () => {
    vi.mocked(eventService.getEvents).mockResolvedValue(mockEventList);

    const { result } = renderHook(() => useEvents(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockEventList);
    expect(eventService.getEvents).toHaveBeenCalledTimes(1);
  });

  it('passes query params to service', async () => {
    vi.mocked(eventService.getEvents).mockResolvedValue(mockEventList);
    const params = { page: 2, pageSize: 20 };

    const { result } = renderHook(() => useEvents(params), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(eventService.getEvents).toHaveBeenCalledWith(params);
  });

  it('handles error state', async () => {
    const error = new Error('Failed to fetch events');
    vi.mocked(eventService.getEvents).mockRejectedValue(error);

    const { result } = renderHook(() => useEvents(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toEqual(error);
  });
});

describe('useEvent', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches single event by id', async () => {
    vi.mocked(eventService.getEventById).mockResolvedValue(mockEvent);

    const { result } = renderHook(() => useEvent(1), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockEvent);
    expect(eventService.getEventById).toHaveBeenCalledWith(1);
  });

  it('does not fetch when id is 0', async () => {
    const { result } = renderHook(() => useEvent(0), {
      wrapper: createWrapper(),
    });

    // Query should be disabled
    expect(result.current.fetchStatus).toBe('idle');
    expect(eventService.getEventById).not.toHaveBeenCalled();
  });
});

describe('useCreateEvent', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('creates event successfully', async () => {
    vi.mocked(eventService.createEvent).mockResolvedValue(mockEvent);

    const { result } = renderHook(() => useCreateEvent(), {
      wrapper: createWrapper(),
    });

    const createData = {
      title: 'New Event',
      description: 'Test',
      location: 'Test Location',
      startTime: '2024-03-01T10:00:00',
      durationMinutes: 60,
      capacity: 20,
    };

    await act(async () => {
      result.current.mutate(createData);
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(eventService.createEvent).toHaveBeenCalledWith(createData, expect.anything());
  });

  it('handles create error', async () => {
    const error = new Error('Create failed');
    vi.mocked(eventService.createEvent).mockRejectedValue(error);

    const { result } = renderHook(() => useCreateEvent(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate({
        title: 'Test',
        description: '',
        location: '',
        startTime: '',
        durationMinutes: 60,
        capacity: 10,
      });
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toEqual(error);
  });
});

describe('useUpdateEvent', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('updates event successfully', async () => {
    const updatedEvent = { ...mockEvent, title: 'Updated Title' };
    vi.mocked(eventService.updateEvent).mockResolvedValue(updatedEvent);

    const { result } = renderHook(() => useUpdateEvent(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate({ id: 1, data: { title: 'Updated Title' } });
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // mutationFn destructures and calls: ({ id, data }) => updateEvent(id, data)
    expect(eventService.updateEvent).toHaveBeenCalledWith(1, { title: 'Updated Title' });
  });
});

describe('useDeleteEvent', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deletes event successfully', async () => {
    vi.mocked(eventService.deleteEvent).mockResolvedValue(undefined);

    const { result } = renderHook(() => useDeleteEvent(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate(1);
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(eventService.deleteEvent).toHaveBeenCalledWith(1, expect.anything());
  });
});

describe('useCancelEvent', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('cancels event successfully', async () => {
    const cancelledEvent = { ...mockEvent, status: 'Cancelled' };
    vi.mocked(eventService.cancelEvent).mockResolvedValue(cancelledEvent);

    const { result } = renderHook(() => useCancelEvent(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate(1);
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(eventService.cancelEvent).toHaveBeenCalledWith(1, expect.anything());
    expect(result.current.data?.status).toBe('Cancelled');
  });
});
