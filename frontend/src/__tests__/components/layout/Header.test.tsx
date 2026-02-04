import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { BrowserRouter, MemoryRouter } from 'react-router-dom'
import { Header } from '@/components/layout/Header'
import { AuthProvider } from '@/context/AuthContext'
import { UserRole } from '@/types/enums'
import type { User } from '@/types'

const mockNavigate = vi.fn()

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

function renderHeader(initialPath = '/') {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <AuthProvider>
        <Header />
      </AuthProvider>
    </MemoryRouter>
  )
}

describe('Header', () => {
  beforeEach(() => {
    mockNavigate.mockClear()
    localStorage.clear()
  })

  it('renders logo and brand name', () => {
    renderHeader()
    
    expect(screen.getByLabelText('Volunteer Event Portal Logo')).toBeInTheDocument()
    expect(screen.getByText('Volunteer Portal')).toBeInTheDocument()
  })

  it('renders public navigation links when not authenticated', () => {
    renderHeader()
    
    expect(screen.getByRole('link', { name: 'Home' })).toHaveAttribute('href', '/')
    expect(screen.getByRole('link', { name: 'Events' })).toHaveAttribute('href', '/events')
    expect(screen.getByRole('link', { name: 'Login' })).toHaveAttribute('href', '/login')
    expect(screen.getByRole('link', { name: 'Sign Up' })).toHaveAttribute('href', '/register')
  })

  it('highlights active navigation link', () => {
    renderHeader('/events')
    
    const eventsLink = screen.getByRole('link', { name: 'Events' })
    expect(eventsLink).toHaveClass('text-blue-600')
  })

  it('toggles mobile menu when hamburger button clicked', async () => {
    renderHeader()
    
    const hamburgerButton = screen.getByLabelText('Toggle navigation menu')
    
    fireEvent.click(hamburgerButton)
    
    await waitFor(() => {
      const mobileLinks = screen.getAllByRole('link', { name: 'Home' })
      expect(mobileLinks.length).toBeGreaterThan(1)
    })
  })

  it('closes mobile menu when navigation link clicked', async () => {
    renderHeader()
    
    const hamburgerButton = screen.getByLabelText('Toggle navigation menu')
    fireEvent.click(hamburgerButton)
    
    await waitFor(() => {
      const mobileHomeLinks = screen.getAllByRole('link', { name: 'Home' })
      expect(mobileHomeLinks.length).toBeGreaterThan(1)
    })
    
    const mobileHomeLink = screen.getAllByRole('link', { name: 'Home' })[1]
    fireEvent.click(mobileHomeLink)
  })
})
