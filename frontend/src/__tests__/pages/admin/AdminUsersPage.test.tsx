import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import AdminUsersPage from '@/pages/admin/AdminUsersPage';
import * as adminService from '@/services/adminService';
import * as useAuthHook from '@/hooks/useAuth';
import { toast } from '@/utils/toast';
import { UserRole } from '@/types/enums';

// Mock the admin service
vi.mock('@/services/adminService');
vi.mock('@/utils/toast');
vi.mock('@/hooks/useAuth');

const mockUsers: adminService.AdminUserResponse[] = [
  {
    id: 1,
    name: 'Admin User',
    email: 'admin@example.com',
    role: UserRole.Admin,
    roleName: 'Admin',
    isDeleted: false,
    createdAt: '2024-01-15T10:00:00Z',
    updatedAt: '2024-01-20T12:00:00Z',
  },
  {
    id: 2,
    name: 'John Volunteer',
    email: 'john@example.com',
    role: UserRole.Volunteer,
    roleName: 'Volunteer',
    isDeleted: false,
    createdAt: '2024-02-10T08:00:00Z',
    updatedAt: null,
  },
  {
    id: 3,
    name: 'Jane Organizer',
    email: 'jane@example.com',
    role: UserRole.Organizer,
    roleName: 'Organizer',
    isDeleted: false,
    createdAt: '2024-03-05T14:00:00Z',
    updatedAt: null,
  },
  {
    id: 4,
    name: 'Deleted User',
    email: 'deleted@example.com',
    role: UserRole.Volunteer,
    roleName: 'Volunteer',
    isDeleted: true,
    createdAt: '2024-01-01T09:00:00Z',
    updatedAt: '2024-03-01T10:00:00Z',
  },
];

const mockUserListResponse: adminService.AdminUserListResponse = {
  users: mockUsers,
  page: 1,
  pageSize: 10,
  totalCount: 4,
  totalPages: 1,
  hasPreviousPage: false,
  hasNextPage: false,
};

const mockCurrentUser = {
  id: 1,
  name: 'Admin User',
  email: 'admin@example.com',
  role: UserRole.Admin,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
};

function createQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

function renderWithProviders(component: React.ReactElement) {
  const queryClient = createQueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{component}</BrowserRouter>
    </QueryClientProvider>
  );
}

describe('AdminUsersPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useAuthHook.useAuth).mockReturnValue({
      user: mockCurrentUser,
      token: 'mock-token',
      isAuthenticated: true,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
      register: vi.fn(),
      refetchUser: vi.fn(),
    });
    vi.mocked(adminService.getAdminUsers).mockResolvedValue(mockUserListResponse);
  });

  it('renders page title and description', async () => {
    renderWithProviders(<AdminUsersPage />);

    expect(screen.getByText('User Management')).toBeInTheDocument();
    expect(screen.getByText('View and manage all platform users')).toBeInTheDocument();
  });

  it('displays loading skeletons initially', () => {
    vi.mocked(adminService.getAdminUsers).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    renderWithProviders(<AdminUsersPage />);

    const loadingSkeletons = screen
      .getAllByRole('generic')
      .filter((el) => el.className.includes('animate-pulse'));
    expect(loadingSkeletons.length).toBeGreaterThan(0);
  });

  it('displays users table when data loads', async () => {
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('Admin User')).toBeInTheDocument();
      expect(screen.getByText('John Volunteer')).toBeInTheDocument();
      expect(screen.getByText('Jane Organizer')).toBeInTheDocument();
      expect(screen.getByText('Deleted User')).toBeInTheDocument();
    });
  });

  it('displays user emails in table', async () => {
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('admin@example.com')).toBeInTheDocument();
      expect(screen.getByText('john@example.com')).toBeInTheDocument();
      expect(screen.getByText('jane@example.com')).toBeInTheDocument();
    });
  });

  it('displays role badges for users', async () => {
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      // There are multiple Admin/Volunteer/Organizer badges
      expect(screen.getAllByText('Admin').length).toBeGreaterThan(0);
      expect(screen.getAllByText('Volunteer').length).toBeGreaterThan(0);
      expect(screen.getByText('Organizer')).toBeInTheDocument();
    });
  });

  it('displays status badges for users', async () => {
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      const activeStatuses = screen.getAllByText('Active');
      const deletedStatuses = screen.getAllByText('Deleted');
      expect(activeStatuses.length).toBe(3);
      expect(deletedStatuses.length).toBe(1);
    });
  });

  it('indicates current user with "(You)" label', async () => {
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('(You)')).toBeInTheDocument();
    });
  });

  it('search input filters users', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('Admin User')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText('Search by name or email...');
    await user.type(searchInput, 'john');

    // getAdminUsers should be called with search parameter after debounce
    await waitFor(
      () => {
        expect(adminService.getAdminUsers).toHaveBeenCalledWith(
          expect.objectContaining({ search: 'john' })
        );
      },
      { timeout: 500 }
    );
  });

  it('status filter changes query params', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('Admin User')).toBeInTheDocument();
    });

    const statusSelect = screen.getByRole('combobox');
    await user.selectOptions(statusSelect, 'active');

    await waitFor(() => {
      expect(adminService.getAdminUsers).toHaveBeenCalledWith(
        expect.objectContaining({ status: 'active' })
      );
    });
  });

  it('disables action buttons for current user', async () => {
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('Admin User')).toBeInTheDocument();
    });

    // Find the row with current user and check buttons are disabled
    const buttons = screen.getAllByRole('button');
    const editButtons = buttons.filter((btn) => btn.title === 'Cannot change own role');
    const deleteButtons = buttons.filter((btn) => btn.title === 'Cannot delete yourself');

    expect(editButtons.length).toBeGreaterThan(0);
    expect(deleteButtons.length).toBeGreaterThan(0);
  });

  it('disables action buttons for deleted users', async () => {
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('Deleted User')).toBeInTheDocument();
    });

    const buttons = screen.getAllByRole('button');
    const disabledEditButtons = buttons.filter((btn) => btn.title === 'Cannot modify deleted user');
    const alreadyDeletedButtons = buttons.filter((btn) => btn.title === 'Already deleted');

    expect(disabledEditButtons.length).toBeGreaterThan(0);
    expect(alreadyDeletedButtons.length).toBeGreaterThan(0);
  });

  it('opens role change modal when clicking edit button', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('John Volunteer')).toBeInTheDocument();
    });

    // Find the edit button for John Volunteer (not the current user)
    const editButtons = screen.getAllByTitle('Change role');
    await user.click(editButtons[0]);

    await waitFor(() => {
      expect(screen.getByText('Change User Role')).toBeInTheDocument();
      expect(screen.getByText('New Role')).toBeInTheDocument();
    });
  });

  it('opens delete confirmation modal when clicking delete button', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('John Volunteer')).toBeInTheDocument();
    });

    const deleteButtons = screen.getAllByTitle('Delete user');
    await user.click(deleteButtons[0]);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Delete User' })).toBeInTheDocument();
      expect(screen.getByText(/Are you sure you want to delete/)).toBeInTheDocument();
    });
  });

  it('calls updateUserRole when confirming role change', async () => {
    const user = userEvent.setup();
    vi.mocked(adminService.updateUserRole).mockResolvedValue({
      id: 1,
      email: 'test@example.com',
      name: 'Test User',
      role: 1,
      roleName: 'Organizer',
      isDeleted: false,
      createdAt: '2024-01-01T00:00:00',
      updatedAt: '2024-01-01T00:00:00',
    });

    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('John Volunteer')).toBeInTheDocument();
    });

    // Open modal for John Volunteer
    const editButtons = screen.getAllByTitle('Change role');
    await user.click(editButtons[0]);

    await waitFor(() => {
      expect(screen.getByText('Change User Role')).toBeInTheDocument();
    });

    // Change role to Organizer
    const roleSelect = screen.getByLabelText('New Role');
    await user.selectOptions(roleSelect, String(UserRole.Organizer));

    // Click Update Role button
    const updateButton = screen.getByText('Update Role');
    await user.click(updateButton);

    await waitFor(() => {
      expect(adminService.updateUserRole).toHaveBeenCalledWith(2, { role: UserRole.Organizer });
      expect(toast.success).toHaveBeenCalledWith(expect.stringContaining('Role updated'));
    });
  });

  it('calls deleteUser when confirming delete', async () => {
    const user = userEvent.setup();
    vi.mocked(adminService.deleteUser).mockResolvedValue({ message: 'User deleted successfully' });

    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('John Volunteer')).toBeInTheDocument();
    });

    // Open delete modal for John Volunteer
    const deleteButtons = screen.getAllByTitle('Delete user');
    await user.click(deleteButtons[0]);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Delete User' })).toBeInTheDocument();
    });

    // Click Delete User button (the one in the modal, not the heading)
    const confirmButton = screen.getByRole('button', { name: 'Delete User' });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(adminService.deleteUser).toHaveBeenCalledWith(2);
      expect(toast.success).toHaveBeenCalledWith(expect.stringContaining('has been deleted'));
    });
  });

  it('closes modal when clicking cancel', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('John Volunteer')).toBeInTheDocument();
    });

    // Open modal
    const editButtons = screen.getAllByTitle('Change role');
    await user.click(editButtons[0]);

    await waitFor(() => {
      expect(screen.getByText('Change User Role')).toBeInTheDocument();
    });

    // Click Cancel
    const cancelButton = screen.getByText('Cancel');
    await user.click(cancelButton);

    await waitFor(() => {
      expect(screen.queryByText('Change User Role')).not.toBeInTheDocument();
    });
  });

  it('displays error state when loading fails', async () => {
    vi.mocked(adminService.getAdminUsers).mockRejectedValue(new Error('Failed to fetch'));

    renderWithProviders(<AdminUsersPage />);

    await waitFor(
      () => {
        expect(
          screen.getByText('Failed to load users. Please try again later.')
        ).toBeInTheDocument();
      },
      { timeout: 3000 }
    );
  });

  it('displays retry button on error', async () => {
    vi.mocked(adminService.getAdminUsers).mockRejectedValue(new Error('Failed to fetch'));

    renderWithProviders(<AdminUsersPage />);

    await waitFor(
      () => {
        expect(screen.getByText('Retry')).toBeInTheDocument();
      },
      { timeout: 3000 }
    );
  });

  it('displays empty state when no users match', async () => {
    vi.mocked(adminService.getAdminUsers).mockResolvedValue({
      ...mockUserListResponse,
      users: [],
      totalCount: 0,
    });

    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('No users found matching your criteria.')).toBeInTheDocument();
    });
  });

  it('displays pagination when there are multiple pages', async () => {
    vi.mocked(adminService.getAdminUsers).mockResolvedValue({
      ...mockUserListResponse,
      totalCount: 25,
      totalPages: 3,
      hasNextPage: true,
    });

    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('Previous')).toBeInTheDocument();
      expect(screen.getByText('Next')).toBeInTheDocument();
      expect(screen.getByText('Page 1 of 3')).toBeInTheDocument();
    });
  });

  it('navigates to next page when clicking Next', async () => {
    const user = userEvent.setup();
    vi.mocked(adminService.getAdminUsers).mockResolvedValue({
      ...mockUserListResponse,
      totalCount: 25,
      totalPages: 3,
      hasNextPage: true,
    });

    renderWithProviders(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText('Next')).toBeInTheDocument();
    });

    const nextButton = screen.getByText('Next');
    await user.click(nextButton);

    await waitFor(() => {
      expect(adminService.getAdminUsers).toHaveBeenCalledWith(expect.objectContaining({ page: 2 }));
    });
  });
});
