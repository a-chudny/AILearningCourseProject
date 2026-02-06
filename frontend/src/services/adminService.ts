import { api } from './api';

/**
 * Admin statistics response - matches AdminStatsResponse DTO
 */
export interface AdminStatsResponse {
  totalUsers: number;
  totalEvents: number;
  totalRegistrations: number;
  registrationsThisMonth: number;
  upcomingEvents: number;
}

/**
 * Admin user response - matches AdminUserResponse DTO
 */
export interface AdminUserResponse {
  id: number;
  name: string;
  email: string;
  role: number;
  roleName: string;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string | null;
}

/**
 * Admin user list response - matches AdminUserListResponse DTO
 */
export interface AdminUserListResponse {
  users: AdminUserResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Query parameters for admin users list
 */
export interface AdminUsersQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  includeDeleted?: boolean;
  status?: 'active' | 'deleted' | null;
}

/**
 * Update user role request
 */
export interface UpdateUserRoleRequest {
  role: number;
}

/**
 * Get admin dashboard statistics
 * Requires Admin role authentication
 */
export async function getAdminStats(): Promise<AdminStatsResponse> {
  const response = await api.get<AdminStatsResponse>('/admin/stats');
  return response.data;
}

/**
 * Get paginated list of users for admin management
 * Requires Admin role authentication
 */
export async function getAdminUsers(
  params?: AdminUsersQueryParams
): Promise<AdminUserListResponse> {
  const response = await api.get<AdminUserListResponse>('/admin/users', { params });
  return response.data;
}

/**
 * Update a user's role
 * Requires Admin role authentication
 */
export async function updateUserRole(
  userId: number,
  request: UpdateUserRoleRequest
): Promise<AdminUserResponse> {
  const response = await api.put<AdminUserResponse>(`/admin/users/${userId}/role`, request);
  return response.data;
}

/**
 * Soft delete a user
 * Requires Admin role authentication
 */
export async function deleteUser(userId: number): Promise<{ message: string }> {
  const response = await api.delete<{ message: string }>(`/admin/users/${userId}`);
  return response.data;
}

/**
 * Query parameters for admin events list
 */
export interface AdminEventsQueryParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  includePastEvents?: boolean;
  includeDeleted?: boolean;
  status?: number | null; // 0 = Active, 1 = Cancelled
}

/**
 * Get paginated list of events for admin management
 * Requires Admin role authentication
 */
export async function getAdminEvents(params?: AdminEventsQueryParams): Promise<any> {
  const response = await api.get('/events', { params });
  return response.data;
}

/**
 * Get registrations for a specific event
 * Requires Admin or Organizer role authentication
 */
export async function getEventRegistrations(eventId: number): Promise<any[]> {
  const response = await api.get(`/events/${eventId}/registrations`);
  return response.data;
}

/**
 * Export filters for registrations
 */
export interface RegistrationExportFilters {
  startDate?: string; // ISO date string
  endDate?: string; // ISO date string
}

/**
 * Export all users to CSV
 * Requires Admin role authentication
 */
export async function exportUsers(): Promise<Blob> {
  const response = await api.get('/reports/users/export', {
    responseType: 'blob',
  });
  return response.data;
}

/**
 * Export all events to CSV
 * Requires Admin role authentication
 */
export async function exportEvents(): Promise<Blob> {
  const response = await api.get('/reports/events/export', {
    responseType: 'blob',
  });
  return response.data;
}

/**
 * Export all registrations to CSV with optional date filters
 * Requires Admin role authentication
 */
export async function exportRegistrations(filters?: RegistrationExportFilters): Promise<Blob> {
  const response = await api.get('/reports/registrations/export', {
    params: filters,
    responseType: 'blob',
  });
  return response.data;
}

/**
 * Export skills summary to CSV
 * Requires Admin role authentication
 */
export async function exportSkillsSummary(): Promise<Blob> {
  const response = await api.get('/reports/skills/export', {
    responseType: 'blob',
  });
  return response.data;
}
