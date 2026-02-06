import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useMyRegistrations, useCancelRegistration } from '@/hooks/useRegistrations';
import * as registrationService from '@/services/registrationService';
import { toast } from '@/utils/toast';
import { RegistrationStatus, EventStatus } from '@/types/enums';

// Mock the services
vi.mock('@/services/registrationService');
vi.mock('@/utils/toast', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
    info: vi.fn(),
    warning: vi.fn(),
  },
}));

const mockRegistrations = [
  {
    id: 1,
    userId: 1,
    eventId: 10,
    status: 0 as RegistrationStatus,
    registeredAt: '2024-02-15T00:00:00',
    event: {
      id: 10,
      title: 'Beach Cleanup',
      location: 'Beach Location',
      startTime: '2024-03-01T10:00:00',
      durationMinutes: 180,
      status: 0 as EventStatus,
      imageUrl: undefined,
    },
  },
  {
    id: 2,
    userId: 1,
    eventId: 20,
    status: 0 as RegistrationStatus,
    registeredAt: '2024-02-20T00:00:00',
    event: {
      id: 20,
      title: 'Food Bank Volunteering',
      location: 'Food Bank Location',
      startTime: '2024-03-15T09:00:00',
      durationMinutes: 120,
      status: 0 as EventStatus,
      imageUrl: undefined,
    },
  },
];

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

describe('useMyRegistrations', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches user registrations successfully', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue(mockRegistrations);

    const { result } = renderHook(() => useMyRegistrations(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockRegistrations);
    expect(registrationService.getMyRegistrations).toHaveBeenCalledTimes(1);
  });

  it('handles error state', async () => {
    const error = new Error('Failed to fetch registrations');
    vi.mocked(registrationService.getMyRegistrations).mockRejectedValue(error);

    const { result } = renderHook(() => useMyRegistrations(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toEqual(error);
  });

  it('returns empty array when no registrations', async () => {
    vi.mocked(registrationService.getMyRegistrations).mockResolvedValue([]);

    const { result } = renderHook(() => useMyRegistrations(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual([]);
  });
});

describe('useCancelRegistration', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('cancels registration successfully', async () => {
    vi.mocked(registrationService.cancelRegistration).mockResolvedValue(undefined);

    const { result } = renderHook(() => useCancelRegistration(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate(10);
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // mutationFn wraps the call: (eventId) => cancelRegistration(eventId)
    expect(registrationService.cancelRegistration).toHaveBeenCalledWith(10);
    expect(toast.success).toHaveBeenCalledWith('Registration cancelled successfully');
  });

  it('shows error toast on failure', async () => {
    const error = new Error('Cannot cancel registration');
    vi.mocked(registrationService.cancelRegistration).mockRejectedValue(error);

    const { result } = renderHook(() => useCancelRegistration(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate(10);
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(toast.error).toHaveBeenCalledWith('Cannot cancel registration');
  });

  it('shows default error message when error has no message', async () => {
    vi.mocked(registrationService.cancelRegistration).mockRejectedValue(new Error());

    const { result } = renderHook(() => useCancelRegistration(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate(10);
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(toast.error).toHaveBeenCalledWith('Failed to cancel registration');
  });
});
