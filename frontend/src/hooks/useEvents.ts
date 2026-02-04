import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { EventQueryParams } from '@/types/api';
import {
  getEvents,
  getEventById,
  createEvent,
  deleteEvent,
  updateEvent,
  type CreateEventRequest,
  type UpdateEventRequest,
  type EventResponse,
  type EventListResponse,
} from '@/services/eventService';

/**
 * Query key factory for events
 */
export const eventKeys = {
  all: ['events'] as const,
  lists: () => [...eventKeys.all, 'list'] as const,
  list: (params?: EventQueryParams) => [...eventKeys.lists(), params] as const,
  details: () => [...eventKeys.all, 'detail'] as const,
  detail: (id: number) => [...eventKeys.details(), id] as const,
};

/**
 * Hook for fetching paginated events list
 */
export function useEvents(params?: EventQueryParams) {
  return useQuery<EventListResponse>({
    queryKey: eventKeys.list(params),
    queryFn: () => getEvents(params),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Hook for fetching a single event by ID
 */
export function useEvent(id: number) {
  return useQuery<EventResponse>({
    queryKey: eventKeys.detail(id),
    queryFn: () => getEventById(id),
    enabled: !!id,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Hook for creating a new event
 */
export function useCreateEvent() {
  const queryClient = useQueryClient();

  return useMutation<EventResponse, Error, CreateEventRequest>({
    mutationFn: createEvent,
    onSuccess: () => {
      // Invalidate all event lists to refetch with new event
      queryClient.invalidateQueries({ queryKey: eventKeys.lists() });
    },
  });
}

/**
 * Hook for updating an event
 */
export function useUpdateEvent() {
  const queryClient = useQueryClient();

  return useMutation<EventResponse, Error, { id: number; data: UpdateEventRequest }>({
    mutationFn: ({ id, data }) => updateEvent(id, data),
    onSuccess: (data) => {
      // Invalidate the specific event and all lists
      queryClient.invalidateQueries({ queryKey: eventKeys.detail(data.id) });
      queryClient.invalidateQueries({ queryKey: eventKeys.lists() });
    },
  });
}

/**
 * Hook for deleting an event
 */
export function useDeleteEvent() {
  const queryClient = useQueryClient();

  return useMutation<void, Error, number>({
    mutationFn: deleteEvent,
    onSuccess: () => {
      // Invalidate all event lists
      queryClient.invalidateQueries({ queryKey: eventKeys.lists() });
    },
  });
}
