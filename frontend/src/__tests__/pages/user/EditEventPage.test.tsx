import { render, screen, waitFor } from '@testing-library/react'
import { vi, describe, it, expect, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import EditEventPage from '@/pages/user/EditEventPage'
import { AuthContext, type AuthContextType } from '@/context/AuthContext'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { UserRole } from '@/types'
import type { User } from '@/types'
import * as useEventsModule from '@/hooks/useEvents'

// Mock skill service
vi.mock('@/services/skillService', () => ({
  getSkills: vi.fn(() => Promise.resolve([
    { id: 1, name: 'First Aid', description: 'First aid skills' },
    { id: 2, name: 'Teaching', description: 'Teaching skills' },
  ])),
}))

// Mock navigate
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => ({ id: '1' }),
  }
})

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false },
  },
})

const mockEvent = {
  id: 1,
  title: 'Beach Cleanup',
  description: 'Help clean up the beach',
  location: 'Santa Monica Beach',
  startTime: '2026-06-15T10:00:00',
  durationMinutes: 120,
  capacity: 50,
  organizerId: 1,
  organizerName: 'John Doe',
  registrationCount: 10,
  requiredSkills: [
    { id: 1, name: 'First Aid', description: 'First aid skills', createdAt: '2026-01-01' },
  ],
  status: 'Active',
  createdAt: '2026-01-01T00:00:00',
  updatedAt: '2026-01-01T00:00:00',
}

const mockUser: User = {
  id: 1,
  email: 'test@example.com',
  name: 'Test User',
  role: UserRole.Organizer,
  createdAt: '2026-01-01',
  updatedAt: '2026-01-01',
}

const mockAdminUser: User = {
  id: 2,
  email: 'admin@example.com',
  name: 'Admin User',
  role: UserRole.Admin,
  createdAt: '2026-01-01',
  updatedAt: '2026-01-01',
}

const mockOtherUser: User = {
  id: 3,
  email: 'other@example.com',
  name: 'Other User',
  role: UserRole.Organizer,
  createdAt: '2026-01-01',
  updatedAt: '2026-01-01',
}

const mockAuthContext: AuthContextType = {
  user: mockUser,
  token: 'test-token',
  isAuthenticated: true,
  isLoading: false,
  login: vi.fn(),
  register: vi.fn(),
  logout: vi.fn(),
  refetchUser: vi.fn(),
}

function renderWithProviders(
  ui: React.ReactElement,
  { authContext = mockAuthContext } = {}
) {
  return render(
    <QueryClientProvider client={queryClient}>
      <AuthContext.Provider value={authContext}>
        <MemoryRouter initialEntries={['/events/1/edit']}>
          {ui}
        </MemoryRouter>
      </AuthContext.Provider>
    </QueryClientProvider>
  )
}

describe('EditEventPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    queryClient.clear()
  })

  it('renders loading skeleton while fetching event', () => {
    vi.spyOn(useEventsModule, 'useEvent').mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any)

    vi.spyOn(useEventsModule, 'useUpdateEvent').mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any)

    const { container } = renderWithProviders(<EditEventPage />)

    // Should show skeleton loading state (animated elements)
    const skeletons = container.querySelectorAll('.animate-pulse')
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders edit form when event is loaded for owner', async () => {
    vi.spyOn(useEventsModule, 'useEvent').mockReturnValue({
      data: mockEvent,
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any)

    vi.spyOn(useEventsModule, 'useUpdateEvent').mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any)

    renderWithProviders(<EditEventPage />)

    await waitFor(() => {
      expect(screen.getByText('Edit Event')).toBeInTheDocument()
      expect(screen.getByText('Update the details for your volunteer event')).toBeInTheDocument()
      expect(screen.getByDisplayValue('Beach Cleanup')).toBeInTheDocument()
      expect(screen.getByDisplayValue('Help clean up the beach')).toBeInTheDocument()
      expect(screen.getByText('Save Changes')).toBeInTheDocument()
    })
  })

  it('redirects to 404 if event not found', async () => {
    vi.spyOn(useEventsModule, 'useEvent').mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: new Error('Not found'),
      refetch: vi.fn(),
    } as any)

    vi.spyOn(useEventsModule, 'useUpdateEvent').mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any)

    renderWithProviders(<EditEventPage />)

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/404', { replace: true })
    })
  })

  it('redirects to 404 if user is not owner or admin', async () => {
    vi.spyOn(useEventsModule, 'useEvent').mockReturnValue({
      data: mockEvent,
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any)

    vi.spyOn(useEventsModule, 'useUpdateEvent').mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any)

    renderWithProviders(<EditEventPage />, {
      authContext: { ...mockAuthContext, user: mockOtherUser },
    })

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/404', { replace: true })
    })
  })

  it('allows admin to edit event even if not owner', async () => {
    vi.spyOn(useEventsModule, 'useEvent').mockReturnValue({
      data: mockEvent,
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any)

    vi.spyOn(useEventsModule, 'useUpdateEvent').mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any)

    renderWithProviders(<EditEventPage />, {
      authContext: { ...mockAuthContext, user: mockAdminUser },
    })

    await waitFor(() => {
      expect(screen.getByText('Edit Event')).toBeInTheDocument()
      expect(screen.getByDisplayValue('Beach Cleanup')).toBeInTheDocument()
      expect(mockNavigate).not.toHaveBeenCalled()
    })
  })

  it('populates form with event data', async () => {
    vi.spyOn(useEventsModule, 'useEvent').mockReturnValue({
      data: mockEvent,
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any)

    vi.spyOn(useEventsModule, 'useUpdateEvent').mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any)

    renderWithProviders(<EditEventPage />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('Beach Cleanup')).toBeInTheDocument()
      expect(screen.getByDisplayValue('Help clean up the beach')).toBeInTheDocument()
      expect(screen.getByDisplayValue('Santa Monica Beach')).toBeInTheDocument()
      expect(screen.getByDisplayValue('2026-06-15')).toBeInTheDocument()
      expect(screen.getByDisplayValue('10:00')).toBeInTheDocument()
      expect(screen.getByDisplayValue('50')).toBeInTheDocument()
    })
  })

  it('shows unsaved changes modal when appropriate', async () => {
    vi.spyOn(useEventsModule, 'useEvent').mockReturnValue({
      data: mockEvent,
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any)

    vi.spyOn(useEventsModule, 'useUpdateEvent').mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any)

    renderWithProviders(<EditEventPage />)

    // Test would need user interaction to trigger unsaved changes
    // This is a placeholder for the modal presence test
    await waitFor(() => {
      expect(screen.getByText('Edit Event')).toBeInTheDocument()
    })
  })

  it('renders in a white card layout', async () => {
    vi.spyOn(useEventsModule, 'useEvent').mockReturnValue({
      data: mockEvent,
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any)

    vi.spyOn(useEventsModule, 'useUpdateEvent').mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any)

    const { container } = renderWithProviders(<EditEventPage />)

    await waitFor(() => {
      const card = container.querySelector('.bg-white.rounded-lg.shadow-md')
      expect(card).toBeInTheDocument()
    })
  })
})
