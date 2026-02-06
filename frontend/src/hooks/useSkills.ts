import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getSkills, getUserSkills, updateUserSkills } from '@/services/skillService';
import type { Skill } from '@/types';

/**
 * Query key factory for skills
 */
export const skillKeys = {
  all: ['skills'] as const,
  allSkills: () => [...skillKeys.all, 'list'] as const,
  userSkills: () => [...skillKeys.all, 'user'] as const,
};

/**
 * Hook for fetching all available skills
 */
export function useSkills() {
  return useQuery<Skill[]>({
    queryKey: skillKeys.allSkills(),
    queryFn: getSkills,
    staleTime: 1000 * 60 * 10, // 10 minutes - skills don't change often
  });
}

/**
 * Hook for fetching current user's skills
 * Requires authentication
 */
export function useUserSkills() {
  return useQuery<Skill[]>({
    queryKey: skillKeys.userSkills(),
    queryFn: getUserSkills,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Hook for updating current user's skills
 * Invalidates user skills query on success
 */
export function useUpdateUserSkills() {
  const queryClient = useQueryClient();

  return useMutation<void, Error, number[]>({
    mutationFn: updateUserSkills,
    onSuccess: () => {
      // Invalidate user skills to refetch updated data
      queryClient.invalidateQueries({ queryKey: skillKeys.userSkills() });
    },
  });
}
