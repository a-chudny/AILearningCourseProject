import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { AdminSidebar } from '@/components/admin/AdminSidebar'

// Mock useLocation
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useLocation: () => ({ pathname: '/admin' }),
  }
})

describe('AdminSidebar', () => {
  const mockToggle = vi.fn()

  it('renders navigation links', () => {
    render(
      <BrowserRouter>
        <AdminSidebar isCollapsed={false} onToggleCollapse={mockToggle} />
      </BrowserRouter>
    )

    // Use getAllByText because both desktop and mobile navigation render the same labels
    expect(screen.getAllByText('Dashboard').length).toBeGreaterThan(0)
    expect(screen.getAllByText('Users').length).toBeGreaterThan(0)
    expect(screen.getAllByText('Events').length).toBeGreaterThan(0)
    expect(screen.getAllByText('Reports').length).toBeGreaterThan(0)
  })

  it('shows Admin Panel title when not collapsed', () => {
    render(
      <BrowserRouter>
        <AdminSidebar isCollapsed={false} onToggleCollapse={mockToggle} />
      </BrowserRouter>
    )

    expect(screen.getByText('Admin Panel')).toBeInTheDocument()
  })

  it('highlights active link', () => {
    render(
      <BrowserRouter>
        <AdminSidebar isCollapsed={false} onToggleCollapse={mockToggle} />
      </BrowserRouter>
    )

    const dashboardLink = screen.getAllByText('Dashboard')[0].closest('a')
    expect(dashboardLink).toHaveClass('bg-blue-600')
  })
})