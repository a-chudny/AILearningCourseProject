// import { api } from './api'; // TODO: Uncomment when registration API is implemented

/**
 * Registration response from backend API
 */
export interface RegistrationResponse {
  id: number;
  eventId: number;
  userId: number;
  status: string; // RegistrationStatus
  registeredAt: string; // ISO 8601 date-time
  notes?: string;
}

/**
 * Create registration request
 */
export interface CreateRegistrationRequest {
  eventId: number;
  notes?: string;
}

/**
 * Check if current user is registered for an event
 * @placeholder - Returns mock data until registration API is implemented
 */
export async function checkUserRegistration(_eventId: number): Promise<{
  isRegistered: boolean;
  registration?: RegistrationResponse;
}> {
  // TODO: Replace with actual API call when registration endpoints are implemented
  // const response = await api.get<RegistrationResponse>(`/registrations/check/${eventId}`);
  
  // Mock implementation - always returns not registered
  return {
    isRegistered: false,
    registration: undefined,
  };
}

/**
 * Register current user for an event
 * @placeholder - Returns mock data until registration API is implemented
 */
export async function registerForEvent(data: CreateRegistrationRequest): Promise<RegistrationResponse> {
  // TODO: Replace with actual API call when registration endpoints are implemented
  // const response = await api.post<RegistrationResponse>('/registrations', data);
  // return response.data;
  
  // Mock implementation
  return {
    id: Math.floor(Math.random() * 1000),
    eventId: data.eventId,
    userId: 1, // Mock user ID
    status: 'Confirmed',
    registeredAt: new Date().toISOString(),
    notes: data.notes,
  };
}

/**
 * Cancel user's registration for an event
 * @placeholder - Returns void until registration API is implemented
 */
export async function cancelRegistration(_eventId: number): Promise<void> {
  // TODO: Replace with actual API call when registration endpoints are implemented
  // await api.delete(`/registrations/${eventId}`);
  
  // Mock implementation - just return
  return Promise.resolve();
}
