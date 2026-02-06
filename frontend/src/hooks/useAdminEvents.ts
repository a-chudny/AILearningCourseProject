import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getAdminEvents,
  getEventRegistrations,
  type AdminEventsQueryParams,
} from '@/services/adminService';
import { deleteEvent, cancelEvent } from '@/services/eventService';

// Query key factory for admin events
export const adminEventsKeys = {
  all: ['adminEvents'] as const,
  lists: () => [...adminEventsKeys.all, 'list'] as const,
  list: (params: AdminEventsQueryParams) => [...adminEventsKeys.lists(), params] as const,
  registrations: (eventId: number) => [...adminEventsKeys.all, 'registrations', eventId] as const,
};

/**
 * Hook to fetch paginated events for admin management
 * Includes past events and soft-deleted events
 */
export function useAdminEvents(params?: AdminEventsQueryParams) {
  return useQuery({
    queryKey: adminEventsKeys.list(params || {}),
    queryFn: () => getAdminEvents(params),
    staleTime: 1 * 60 * 1000, // 1 minute
  });
}

/**
 * Hook to fetch registrations for a specific event
 */
export function useEventRegistrations(eventId: number | null) {
  return useQuery({
    queryKey: adminEventsKeys.registrations(eventId || 0),
    queryFn: () => {
      if (!eventId) throw new Error('Event ID is required');
      return getEventRegistrations(eventId);
    },
    enabled: eventId !== null,
  });
}

/**
 * Hook to soft delete an event
 * Invalidates admin events cache on success
 */
export function useSoftDeleteEvent() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteEvent,
    onSuccess: () => {
      // Invalidate admin events list
      queryClient.invalidateQueries({ queryKey: adminEventsKeys.lists() });
    },
  });
}

/**
 * Hook to cancel an event
 * Invalidates admin events cache on success
 */
export function useCancelEventMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: cancelEvent,
    onSuccess: () => {
      // Invalidate admin events list
      queryClient.invalidateQueries({ queryKey: adminEventsKeys.lists() });
    },
  });
}
