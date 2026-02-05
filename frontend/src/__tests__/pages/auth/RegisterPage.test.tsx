import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { User } from '@/types/entities'
import { UserRole } from '@/types/enums'
import { BrowserRouter } from 'react-router-dom'
import RegisterPage from '@/pages/auth/RegisterPage'
import { AuthContext } from '@/context/AuthContext'
import type { AuthContextType } from '@/context/AuthContext'

// Helper to create auth context value
const createAuthContextValue = (overrides?: Partial<AuthContextType>): AuthContextType => ({
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: false,
  login: vi.fn(),
  register: vi.fn(),
  logout: vi.fn(),
  refetchUser: vi.fn(),
  ...overrides,
})

describe('RegisterPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  const renderRegisterPage = (contextValue?: Partial<AuthContextType>) => {
    const authContextValue = createAuthContextValue(contextValue)
    return render(
      <BrowserRouter>
        <AuthContext value={authContextValue}>
          <RegisterPage />
        </AuthContext>
      </BrowserRouter>
    )
  }

  it('renders registration form with all fields', () => {
    renderRegisterPage()

    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/email address/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/confirm password/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/age/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /create account/i })).toBeInTheDocument()
  })

  it('shows validation errors for empty required fields', async () => {
    const user = userEvent.setup()
    renderRegisterPage()

    const nameInput = screen.getByLabelText(/full name/i)
    const emailInput = screen.getByLabelText(/email address/i)
    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)
    const ageInput = screen.getByLabelText(/age/i)

    // Focus and blur each field to trigger validation
    await user.click(nameInput)
    await user.tab()
    await user.click(emailInput)
    await user.tab()
    await user.click(passwordInput)
    await user.tab()
    await user.click(confirmPasswordInput)
    await user.tab()
    await user.click(ageInput)
    await user.tab()

    await waitFor(() => {
      expect(screen.getByText(/name is required/i)).toBeInTheDocument()
      expect(screen.getByText(/email is required/i)).toBeInTheDocument()
      expect(screen.getByText(/password is required/i)).toBeInTheDocument()
      expect(screen.getByText(/please confirm your password/i)).toBeInTheDocument()
      expect(screen.getByText(/age is required/i)).toBeInTheDocument()
    })
  })

  it('shows validation error for invalid email format', async () => {
    const user = userEvent.setup()
    renderRegisterPage()

    const emailInput = screen.getByLabelText(/email address/i)

    await user.type(emailInput, 'invalid-email')
    await user.tab()

    await waitFor(() => {
      expect(screen.getByText(/please enter a valid email address/i)).toBeInTheDocument()
    })
  })

  it('shows validation error for password less than 8 characters', async () => {
    const user = userEvent.setup()
    renderRegisterPage()

    const passwordInput = screen.getByLabelText(/^password$/i)

    await user.type(passwordInput, 'pass1')
    await user.tab()

    await waitFor(() => {
      expect(screen.getByText(/password must be at least 8 characters/i)).toBeInTheDocument()
    })
  })

  it('shows validation error for password without a number', async () => {
    const user = userEvent.setup()
    renderRegisterPage()

    const passwordInput = screen.getByLabelText(/^password$/i)

    await user.type(passwordInput, 'password')
    await user.tab()

    await waitFor(() => {
      expect(screen.getByText(/password must contain at least one number/i)).toBeInTheDocument()
    })
  })

  it('shows validation error when passwords do not match', async () => {
    const user = userEvent.setup()
    renderRegisterPage()

    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)

    await user.type(passwordInput, 'password123')
    await user.type(confirmPasswordInput, 'password456')
    await user.tab()

    await waitFor(() => {
      expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument()
    })
  })

  it('shows validation error for age less than 18', async () => {
    const user = userEvent.setup()
    renderRegisterPage()

    const ageInput = screen.getByLabelText(/age/i)

    await user.type(ageInput, '16')
    await user.tab()

    await waitFor(() => {
      expect(screen.getByText(/you must be at least 18 years old to register/i)).toBeInTheDocument()
    })
  })

  it('displays password strength indicator', async () => {
    const user = userEvent.setup()
    renderRegisterPage()

    const passwordInput = screen.getByLabelText(/^password$/i)

    // Type a weak password
    await user.type(passwordInput, 'pass1234')

    await waitFor(() => {
      expect(screen.getByText(/weak|fair/i)).toBeInTheDocument()
    })

    // Clear and type a strong password
    await user.clear(passwordInput)
    await user.type(passwordInput, 'Password123!')

    await waitFor(() => {
      expect(screen.getByText(/good|strong/i)).toBeInTheDocument()
    })
  })

  it('calls register function with correct credentials on valid submission', async () => {
    const user = userEvent.setup()
    const mockRegister = vi.fn().mockResolvedValue(undefined)
    renderRegisterPage({ register: mockRegister })

    // Fill in all fields with valid data
    await user.type(screen.getByLabelText(/full name/i), 'John Doe')
    await user.type(screen.getByLabelText(/email address/i), 'john@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'password123')
    await user.type(screen.getByLabelText(/confirm password/i), 'password123')
    await user.type(screen.getByLabelText(/age/i), '25')

    const submitButton = screen.getByRole('button', { name: /create account/i })
    await user.click(submitButton)

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith({
        name: 'John Doe',
        email: 'john@example.com',
        password: 'password123',
      })
    })
  })

  it('displays error message on registration failure', async () => {
    const user = userEvent.setup()
    const mockRegister = vi.fn().mockRejectedValue(new Error('This email address is already registered. Please use a different email or sign in.'))
    renderRegisterPage({ register: mockRegister })

    await user.type(screen.getByLabelText(/full name/i), 'John Doe')
    await user.type(screen.getByLabelText(/email address/i), 'existing@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'password123')
    await user.type(screen.getByLabelText(/confirm password/i), 'password123')
    await user.type(screen.getByLabelText(/age/i), '25')

    const submitButton = screen.getByRole('button', { name: /create account/i })
    await user.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText(/email address is already registered/i)).toBeInTheDocument()
    })
  })

  it('shows loading state during submission', async () => {
    const user = userEvent.setup()
    const mockRegister = vi.fn().mockImplementation(
      () => new Promise((resolve) => setTimeout(resolve, 1000))
    )
    renderRegisterPage({ register: mockRegister })

    await user.type(screen.getByLabelText(/full name/i), 'John Doe')
    await user.type(screen.getByLabelText(/email address/i), 'john@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'password123')
    await user.type(screen.getByLabelText(/confirm password/i), 'password123')
    await user.type(screen.getByLabelText(/age/i), '25')

    const submitButton = screen.getByRole('button', { name: /create account/i })
    await user.click(submitButton)

    // Check for loading state
    await waitFor(() => {
      expect(screen.getByText(/creating account.../i)).toBeInTheDocument()
      expect(submitButton).toBeDisabled()
    })
  })

  it('should not show loading spinner when authLoading is false and user is authenticated', () => {
    const mockUser: User = {
      id: 1,
      name: 'Test User',
      email: 'test@example.com',
      role: UserRole.Volunteer,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    }

    renderRegisterPage({
      user: mockUser,
      isAuthenticated: true,
      isLoading: false,
    })

    // The component will attempt to redirect via navigate() in useEffect
    // We can't test navigation easily in this setup, but we verify it doesn't show loading
    expect(screen.queryByText(/loading\.\.\./i)).not.toBeInTheDocument()
  })

  it('has a link to login page', () => {
    renderRegisterPage()

    const loginLink = screen.getByRole('link', { name: /sign in/i })
    expect(loginLink).toBeInTheDocument()
    expect(loginLink).toHaveAttribute('href', '/login')
  })

  it('shows hint text for age requirement', () => {
    renderRegisterPage()

    expect(screen.getByText(/you must be at least 18 years old/i)).toBeInTheDocument()
  })

  it('validates all fields on form submission', async () => {
    const user = userEvent.setup()
    const mockRegister = vi.fn()
    renderRegisterPage({ register: mockRegister })

    const submitButton = screen.getByRole('button', { name: /create account/i })
    await user.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText(/name is required/i)).toBeInTheDocument()
      expect(screen.getByText(/email is required/i)).toBeInTheDocument()
      expect(screen.getByText(/password is required/i)).toBeInTheDocument()
      expect(screen.getByText(/please confirm your password/i)).toBeInTheDocument()
      expect(screen.getByText(/age is required/i)).toBeInTheDocument()
    })

    expect(mockRegister).not.toHaveBeenCalled()
  })
})
