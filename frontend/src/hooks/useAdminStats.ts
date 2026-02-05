import { useQuery } from '@tanstack/react-query';
import { getAdminStats, type AdminStatsResponse } from '@/services/adminService';

/**
 * Query key for admin statistics
 */
export const adminStatsKeys = {
  all: ['admin', 'stats'] as const,
};

/**
 * Hook for fetching admin dashboard statistics
 * Requires Admin role authentication
 */
export function useAdminStats() {
  return useQuery<AdminStatsResponse>({
    queryKey: adminStatsKeys.all,
    queryFn: getAdminStats,
    staleTime: 1000 * 60 * 2, // 2 minutes - refresh stats more frequently
  });
}
