import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useSkills, useUserSkills, useUpdateUserSkills, skillKeys } from '@/hooks/useSkills';
import * as skillService from '@/services/skillService';
import type { Skill } from '@/types';

// Mock the skill service
vi.mock('@/services/skillService');

const mockSkills: Skill[] = [
  { id: 1, name: 'React', category: 'Technology', createdAt: '2024-01-01T00:00:00Z' },
  { id: 2, name: 'Node.js', category: 'Technology', createdAt: '2024-01-01T00:00:00Z' },
  { id: 3, name: 'Leadership', category: 'Community', createdAt: '2024-01-01T00:00:00Z' },
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

describe('skillKeys', () => {
  it('creates correct all key', () => {
    expect(skillKeys.all).toEqual(['skills']);
  });

  it('creates correct allSkills key', () => {
    expect(skillKeys.allSkills()).toEqual(['skills', 'list']);
  });

  it('creates correct userSkills key', () => {
    expect(skillKeys.userSkills()).toEqual(['skills', 'user']);
  });
});

describe('useSkills', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches skills successfully', async () => {
    vi.mocked(skillService.getSkills).mockResolvedValue(mockSkills);

    const { result } = renderHook(() => useSkills(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockSkills);
    expect(skillService.getSkills).toHaveBeenCalledTimes(1);
  });

  it('handles error state', async () => {
    const error = new Error('Failed to fetch skills');
    vi.mocked(skillService.getSkills).mockRejectedValue(error);

    const { result } = renderHook(() => useSkills(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toEqual(error);
  });
});

describe('useUserSkills', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches user skills successfully', async () => {
    const userSkills = [mockSkills[0], mockSkills[2]];
    vi.mocked(skillService.getUserSkills).mockResolvedValue(userSkills);

    const { result } = renderHook(() => useUserSkills(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(userSkills);
    expect(skillService.getUserSkills).toHaveBeenCalledTimes(1);
  });
});

describe('useUpdateUserSkills', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('updates user skills successfully', async () => {
    vi.mocked(skillService.updateUserSkills).mockResolvedValue(undefined);

    const { result } = renderHook(() => useUpdateUserSkills(), {
      wrapper: createWrapper(),
    });

    result.current.mutate([1, 3]);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Mutation function receives the data and additional context from React Query
    expect(skillService.updateUserSkills).toHaveBeenCalledWith([1, 3], expect.anything());
  });

  it('handles update error', async () => {
    const error = new Error('Update failed');
    vi.mocked(skillService.updateUserSkills).mockRejectedValue(error);

    const { result } = renderHook(() => useUpdateUserSkills(), {
      wrapper: createWrapper(),
    });

    result.current.mutate([1, 3]);

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toEqual(error);
  });
});
