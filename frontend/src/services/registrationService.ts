import { api, getErrorMessage } from './api';
import { EventStatus, RegistrationStatus } from '@/types/enums';

/**
 * Event summary included in registration response
 */
export interface EventSummary {
  id: number;
  title: string;
  location: string;
  startTime: string;
  durationMinutes: number;
  status: EventStatus;
  imageUrl?: string;
}

/**
 * Registration response from backend API
 */
export interface RegistrationResponse {
  id: number;
  eventId: number;
  userId: number;
  status: RegistrationStatus;
  registeredAt: string; // ISO 8601 date-time
  event: EventSummary;
}

/**
 * Create registration request
 */
export interface CreateRegistrationRequest {
  eventId: number;
  notes?: string;
}

/**
 * Get all registrations for the current user
 */
export async function getMyRegistrations(): Promise<RegistrationResponse[]> {
  const response = await api.get<RegistrationResponse[]>('/users/me/registrations');
  return response.data;
}

/**
 * Check if current user is registered for an event
 * Only returns isRegistered: true for CONFIRMED registrations (not cancelled)
 */
export async function checkUserRegistration(eventId: number): Promise<{
  isRegistered: boolean;
  registration?: RegistrationResponse;
}> {
  try {
    const registrations = await getMyRegistrations();
    // Only consider confirmed registrations (status = 0), not cancelled ones
    const registration = registrations.find(
      (r) => r.eventId === eventId && r.status === RegistrationStatus.Confirmed
    );

    return {
      isRegistered: !!registration,
      registration,
    };
  } catch {
    // If not authenticated or error, return false
    return {
      isRegistered: false,
      registration: undefined,
    };
  }
}

/**
 * Register current user for an event
 */
export async function registerForEvent(
  data: CreateRegistrationRequest
): Promise<RegistrationResponse> {
  try {
    const response = await api.post<RegistrationResponse>(`/events/${data.eventId}/register`, {
      notes: data.notes,
    });
    return response.data;
  } catch (error) {
    throw new Error(getErrorMessage(error));
  }
}

/**
 * Cancel user's registration for an event
 */
export async function cancelRegistration(eventId: number): Promise<void> {
  try {
    await api.delete(`/events/${eventId}/register`);
  } catch (error) {
    throw new Error(getErrorMessage(error));
  }
}
