import { api } from './api';
import type { Skill } from '@/types';
import type { EventQueryParams } from '@/types/api';

/**
 * Event response from backend API - matches EventResponse DTO
 */
export interface EventResponse {
  id: number;
  title: string;
  description: string;
  location: string;
  startTime: string; // ISO 8601 date-time
  durationMinutes: number;
  capacity: number;
  imageUrl?: string;
  registrationDeadline?: string; // ISO 8601 date-time
  status: string; // EventStatus
  organizerId: number;
  organizerName: string;
  registrationCount: number; // Confirmed registrations only
  requiredSkills: Skill[];
  createdAt: string;
  updatedAt: string;
}

/**
 * Event list response from backend - matches EventListResponse DTO
 */
export interface EventListResponse {
  events: EventResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Create event request - matches CreateEventRequest DTO
 */
export interface CreateEventRequest {
  title: string;
  description: string;
  location: string;
  startTime: string; // ISO 8601 date-time
  durationMinutes: number;
  capacity: number;
  imageUrl?: string;
  registrationDeadline?: string; // ISO 8601 date-time
  requiredSkillIds: number[];
}

/**
 * Update event request - matches UpdateEventRequest DTO
 */
export interface UpdateEventRequest extends CreateEventRequest {
  status: string; // EventStatus
}

/**
 * Fetch paginated list of events
 */
export async function getEvents(params?: EventQueryParams): Promise<EventListResponse> {
  const response = await api.get<EventListResponse>('/events', { params });
  return response.data;
}

/**
 * Fetch single event by ID
 */
export async function getEventById(id: number): Promise<EventResponse> {
  const response = await api.get<EventResponse>(`/events/${id}`);
  return response.data;
}

/**
 * Create a new event (Organizer/Admin only)
 */
export async function createEvent(data: CreateEventRequest): Promise<EventResponse> {
  const response = await api.post<EventResponse>('/events', data);
  return response.data;
}

/**
 * Update an existing event (Owner/Admin only)
 */
export async function updateEvent(id: number, data: UpdateEventRequest): Promise<EventResponse> {
  const response = await api.put<EventResponse>(`/events/${id}`, data);
  return response.data;
}

/**
 * Delete an event (Owner/Admin only)
 */
export async function deleteEvent(id: number): Promise<void> {
  await api.delete(`/events/${id}`);
}
