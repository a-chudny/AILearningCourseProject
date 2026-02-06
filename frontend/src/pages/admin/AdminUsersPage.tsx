import { useState, useEffect, useCallback } from 'react';
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  UserCircleIcon,
  TrashIcon,
  PencilIcon,
} from '@heroicons/react/24/outline';
import { useAdminUsers, useUpdateUserRole, useSoftDeleteUser } from '@/hooks/useAdminUsers';
import { useAuth } from '@/hooks/useAuth';
import { toast } from '@/utils/toast';
import { UserRole, UserRoleLabels } from '@/types/enums';
import type { AdminUserResponse, AdminUsersQueryParams } from '@/services/adminService';

// Debounce hook for instant search
function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

interface RoleChangeModalProps {
  user: AdminUserResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (newRole: number) => void;
  isLoading: boolean;
}

function RoleChangeModal({ user, isOpen, onClose, onConfirm, isLoading }: RoleChangeModalProps) {
  const [selectedRole, setSelectedRole] = useState<number>(user?.role ?? 0);

  // Sync selectedRole when user prop changes
  useEffect(() => {
    if (user) {
      // eslint-disable-next-line react-hooks/set-state-in-effect -- syncing prop to local state is an accepted pattern
      setSelectedRole(user.role);
    }
  }, [user]);

  if (!isOpen || !user) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
      <div className="w-full max-w-md rounded-lg bg-white shadow-xl">
        <div className="border-b border-gray-200 px-6 py-4">
          <h2 className="text-xl font-bold text-gray-900">Change User Role</h2>
          <p className="mt-1 text-sm text-gray-600">Update role for {user.name}</p>
        </div>

        <div className="px-6 py-4">
          <div className="mb-4">
            <p className="text-sm text-gray-600">
              Current role: <span className="font-semibold">{user.roleName}</span>
            </p>
          </div>

          <div className="mb-4">
            <label htmlFor="role-select" className="block text-sm font-medium text-gray-700 mb-2">
              New Role
            </label>
            <select
              id="role-select"
              value={selectedRole}
              onChange={(e) => setSelectedRole(Number(e.target.value))}
              className="w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-gray-900 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value={UserRole.Volunteer}>{UserRoleLabels[UserRole.Volunteer]}</option>
              <option value={UserRole.Organizer}>{UserRoleLabels[UserRole.Organizer]}</option>
              <option value={UserRole.Admin}>{UserRoleLabels[UserRole.Admin]}</option>
            </select>
          </div>

          {selectedRole !== user.role && (
            <div className="mb-4 rounded-md bg-yellow-50 border border-yellow-200 p-3">
              <p className="text-sm text-yellow-800">
                <strong>Warning:</strong> Changing this user's role will affect their permissions
                immediately.
                {selectedRole === UserRole.Admin && (
                  <span className="block mt-1">
                    Making this user an Admin will grant them full system access.
                  </span>
                )}
              </p>
            </div>
          )}
        </div>

        <div className="flex justify-end gap-3 border-t border-gray-200 px-6 py-4">
          <button
            type="button"
            onClick={onClose}
            disabled={isLoading}
            className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={() => onConfirm(selectedRole)}
            disabled={isLoading || selectedRole === user.role}
            className="rounded-md bg-blue-600 px-4 py-3 min-h-[44px] text-sm font-medium text-white transition-all hover:bg-blue-700 hover:shadow-md active:scale-95 disabled:opacity-50"
          >
            {isLoading ? 'Updating...' : 'Update Role'}
          </button>
        </div>
      </div>
    </div>
  );
}

interface DeleteConfirmModalProps {
  user: AdminUserResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isLoading: boolean;
}

function DeleteConfirmModal({
  user,
  isOpen,
  onClose,
  onConfirm,
  isLoading,
}: DeleteConfirmModalProps) {
  if (!isOpen || !user) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
      <div className="w-full max-w-md rounded-lg bg-white shadow-xl">
        <div className="border-b border-gray-200 px-6 py-4">
          <h2 className="text-xl font-bold text-red-600">Delete User</h2>
          <p className="mt-1 text-sm text-gray-600">This action cannot be easily undone</p>
        </div>

        <div className="px-6 py-4">
          <p className="text-gray-700">
            Are you sure you want to delete <strong>{user.name}</strong> ({user.email})?
          </p>
          <p className="mt-2 text-sm text-gray-500">
            The user will be marked as deleted and will no longer be able to log in. Their data will
            be preserved but they won't appear in active user lists.
          </p>
        </div>

        <div className="flex justify-end gap-3 border-t border-gray-200 px-6 py-4">
          <button
            type="button"
            onClick={onClose}
            disabled={isLoading}
            className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={isLoading}
            className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
          >
            {isLoading ? 'Deleting...' : 'Delete User'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default function AdminUsersPage() {
  const { user: currentUser } = useAuth();
  const [searchInput, setSearchInput] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'deleted'>('all');
  const [page, setPage] = useState(1);
  const pageSize = 10;

  // Debounce search input for instant search
  const debouncedSearch = useDebounce(searchInput, 300);

  // Build query params
  const queryParams: AdminUsersQueryParams = {
    page,
    pageSize,
    search: debouncedSearch || undefined,
    includeDeleted: true,
    status: statusFilter === 'all' ? null : statusFilter,
  };

  const { data, isLoading, isError, refetch } = useAdminUsers(queryParams);
  const updateRoleMutation = useUpdateUserRole();
  const deleteMutation = useSoftDeleteUser();

  // Modal state
  const [roleModalUser, setRoleModalUser] = useState<AdminUserResponse | null>(null);
  const [deleteModalUser, setDeleteModalUser] = useState<AdminUserResponse | null>(null);

  const handleRoleChange = useCallback(
    async (newRole: number) => {
      if (!roleModalUser) return;

      try {
        await updateRoleMutation.mutateAsync({
          userId: roleModalUser.id,
          request: { role: newRole },
        });
        toast.success(`Role updated successfully for ${roleModalUser.name}`);
        setRoleModalUser(null);
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Failed to update role';
        toast.error(message);
      }
    },
    [roleModalUser, updateRoleMutation]
  );

  const handleDelete = useCallback(async () => {
    if (!deleteModalUser) return;

    try {
      await deleteMutation.mutateAsync(deleteModalUser.id);
      toast.success(`User ${deleteModalUser.name} has been deleted`);
      setDeleteModalUser(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to delete user';
      toast.error(message);
    }
  }, [deleteModalUser, deleteMutation]);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getRoleBadgeColor = (role: number) => {
    switch (role) {
      case UserRole.Admin:
        return 'bg-purple-100 text-purple-800';
      case UserRole.Organizer:
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-green-100 text-green-800';
    }
  };

  if (isError) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">User Management</h1>
          <p className="mt-2 text-gray-600">View and manage all users</p>
        </div>
        <div className="rounded-lg border border-red-200 bg-red-50 p-6">
          <p className="text-red-800">Failed to load users. Please try again later.</p>
          <button
            onClick={() => refetch()}
            className="mt-4 rounded-md bg-red-600 px-4 py-2 text-white hover:bg-red-700"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">User Management</h1>
        <p className="mt-2 text-gray-600">View and manage all platform users</p>
      </div>

      {/* Search and Filter */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by name or email..."
            value={searchInput}
            onChange={(e) => {
              setSearchInput(e.target.value);
              setPage(1);
            }}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 text-gray-900 bg-white focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>

        <div className="flex items-center gap-2">
          <FunnelIcon className="h-5 w-5 text-gray-500" />
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value as 'all' | 'active' | 'deleted');
              setPage(1);
            }}
            className="rounded-md border border-gray-300 px-3 py-2 text-gray-900 bg-white focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            <option value="all">All Users</option>
            <option value="active">Active Only</option>
            <option value="deleted">Deleted Only</option>
          </select>
        </div>
      </div>

      {/* Users Table */}
      <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
                >
                  User
                </th>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
                >
                  Role
                </th>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
                >
                  Status
                </th>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
                >
                  Created
                </th>
                <th
                  scope="col"
                  className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500"
                >
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {isLoading ? (
                // Loading skeleton
                Array.from({ length: 5 }).map((_, index) => (
                  <tr key={index}>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="flex items-center gap-3">
                        <div className="h-10 w-10 animate-pulse rounded-full bg-gray-200" />
                        <div>
                          <div className="h-4 w-32 animate-pulse rounded bg-gray-200" />
                          <div className="mt-1 h-3 w-40 animate-pulse rounded bg-gray-200" />
                        </div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="h-6 w-20 animate-pulse rounded bg-gray-200" />
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="h-6 w-16 animate-pulse rounded bg-gray-200" />
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="h-4 w-24 animate-pulse rounded bg-gray-200" />
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="h-8 w-20 animate-pulse rounded bg-gray-200 ml-auto" />
                    </td>
                  </tr>
                ))
              ) : data?.users.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-6 py-12 text-center text-gray-500">
                    No users found matching your criteria.
                  </td>
                </tr>
              ) : (
                data?.users.map((user) => {
                  const isCurrentUser = currentUser?.id === user.id;
                  return (
                    <tr key={user.id} className={user.isDeleted ? 'bg-red-50' : ''}>
                      <td className="whitespace-nowrap px-6 py-4">
                        <div className="flex items-center gap-3">
                          <UserCircleIcon
                            className={`h-10 w-10 ${user.isDeleted ? 'text-gray-400' : 'text-gray-500'}`}
                          />
                          <div>
                            <div
                              className={`font-medium ${user.isDeleted ? 'text-gray-500 line-through' : 'text-gray-900'}`}
                            >
                              {user.name}
                              {isCurrentUser && (
                                <span className="ml-2 text-xs text-blue-600">(You)</span>
                              )}
                            </div>
                            <div
                              className={`text-sm ${user.isDeleted ? 'text-gray-400' : 'text-gray-500'}`}
                            >
                              {user.email}
                            </div>
                          </div>
                        </div>
                      </td>
                      <td className="whitespace-nowrap px-6 py-4">
                        <span
                          className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${getRoleBadgeColor(user.role)}`}
                        >
                          {user.roleName}
                        </span>
                      </td>
                      <td className="whitespace-nowrap px-6 py-4">
                        {user.isDeleted ? (
                          <span className="inline-flex rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">
                            Deleted
                          </span>
                        ) : (
                          <span className="inline-flex rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">
                            Active
                          </span>
                        )}
                      </td>
                      <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                        {formatDate(user.createdAt)}
                      </td>
                      <td className="whitespace-nowrap px-6 py-4 text-right">
                        <div className="flex justify-end gap-2">
                          <button
                            onClick={() => setRoleModalUser(user)}
                            disabled={isCurrentUser || user.isDeleted}
                            title={
                              isCurrentUser
                                ? 'Cannot change own role'
                                : user.isDeleted
                                  ? 'Cannot modify deleted user'
                                  : 'Change role'
                            }
                            className="rounded-md p-2 text-gray-500 hover:bg-gray-100 hover:text-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <PencilIcon className="h-5 w-5" />
                          </button>
                          <button
                            onClick={() => setDeleteModalUser(user)}
                            disabled={isCurrentUser || user.isDeleted}
                            title={
                              isCurrentUser
                                ? 'Cannot delete yourself'
                                : user.isDeleted
                                  ? 'Already deleted'
                                  : 'Delete user'
                            }
                            className="rounded-md p-2 text-gray-500 hover:bg-gray-100 hover:text-red-600 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <TrashIcon className="h-5 w-5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between border-t border-gray-200 bg-white px-6 py-3">
            <div className="text-sm text-gray-700">
              Showing {(data.page - 1) * data.pageSize + 1} to{' '}
              {Math.min(data.page * data.pageSize, data.totalCount)} of {data.totalCount} users
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => setPage(page - 1)}
                disabled={!data.hasPreviousPage}
                className="rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="flex items-center px-3 text-sm text-gray-700">
                Page {data.page} of {data.totalPages}
              </span>
              <button
                onClick={() => setPage(page + 1)}
                disabled={!data.hasNextPage}
                className="rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Modals */}
      <RoleChangeModal
        user={roleModalUser}
        isOpen={roleModalUser !== null}
        onClose={() => setRoleModalUser(null)}
        onConfirm={handleRoleChange}
        isLoading={updateRoleMutation.isPending}
      />

      <DeleteConfirmModal
        user={deleteModalUser}
        isOpen={deleteModalUser !== null}
        onClose={() => setDeleteModalUser(null)}
        onConfirm={handleDelete}
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}
