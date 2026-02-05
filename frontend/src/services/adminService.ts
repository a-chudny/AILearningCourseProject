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
 * Get admin dashboard statistics
 * Requires Admin role authentication
 */
export async function getAdminStats(): Promise<AdminStatsResponse> {
  const response = await api.get<AdminStatsResponse>('/admin/stats');
  return response.data;
}
