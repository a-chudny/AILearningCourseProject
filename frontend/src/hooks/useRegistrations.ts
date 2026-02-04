import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getMyRegistrations, cancelRegistration } from '@/services/registrationService';
import { toast } from '@/utils/toast';

/**
 * Hook to fetch current user's registrations
 */
export function useMyRegistrations() {
  return useQuery({
    queryKey: ['registrations', 'me'],
    queryFn: getMyRegistrations,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Hook to cancel a registration
 */
export function useCancelRegistration() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (eventId: number) => cancelRegistration(eventId),
    onSuccess: () => {
      // Invalidate registrations list
      queryClient.invalidateQueries({ queryKey: ['registrations', 'me'] });
      // Invalidate events list (to update registration counts)
      queryClient.invalidateQueries({ queryKey: ['events'] });
      toast.success('Registration cancelled successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to cancel registration');
    },
  });
}
