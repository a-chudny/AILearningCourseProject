import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getAdminUsers,
  updateUserRole,
  deleteUser,
  type AdminUserListResponse,
  type AdminUsersQueryParams,
  type UpdateUserRoleRequest,
  type AdminUserResponse,
} from '@/services/adminService';
import { adminStatsKeys } from './useAdminStats';

/**
 * Query key factory for admin users
 */
export const adminUsersKeys = {
  all: ['admin', 'users'] as const,
  list: (params?: AdminUsersQueryParams) => [...adminUsersKeys.all, 'list', params] as const,
};

/**
 * Hook for fetching paginated admin users list
 */
export function useAdminUsers(params?: AdminUsersQueryParams) {
  return useQuery<AdminUserListResponse>({
    queryKey: adminUsersKeys.list(params),
    queryFn: () => getAdminUsers(params),
    staleTime: 1000 * 60 * 1, // 1 minute - refresh more frequently for admin data
  });
}

/**
 * Hook for updating a user's role
 */
export function useUpdateUserRole() {
  const queryClient = useQueryClient();

  return useMutation<AdminUserResponse, Error, { userId: number; request: UpdateUserRoleRequest }>({
    mutationFn: ({ userId, request }) => updateUserRole(userId, request),
    onSuccess: () => {
      // Invalidate users list to refetch with updated data
      queryClient.invalidateQueries({ queryKey: adminUsersKeys.all });
      // Also invalidate stats as user counts might be affected
      queryClient.invalidateQueries({ queryKey: adminStatsKeys.all });
    },
  });
}

/**
 * Hook for soft deleting a user
 */
export function useSoftDeleteUser() {
  const queryClient = useQueryClient();

  return useMutation<{ message: string }, Error, number>({
    mutationFn: (userId) => deleteUser(userId),
    onSuccess: () => {
      // Invalidate users list to refetch with updated data
      queryClient.invalidateQueries({ queryKey: adminUsersKeys.all });
      // Also invalidate stats as user counts are affected
      queryClient.invalidateQueries({ queryKey: adminStatsKeys.all });
    },
  });
}
