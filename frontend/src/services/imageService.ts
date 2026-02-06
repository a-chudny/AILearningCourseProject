import { api } from './api';

/**
 * Upload an image for an event
 */
export async function uploadEventImage(eventId: number, file: File): Promise<{ imageUrl: string }> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await api.post<{ imageUrl: string }>(`/events/${eventId}/image`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });

  return response.data;
}

/**
 * Delete an event's image
 */
export async function deleteEventImage(eventId: number): Promise<void> {
  await api.delete(`/events/${eventId}/image`);
}
