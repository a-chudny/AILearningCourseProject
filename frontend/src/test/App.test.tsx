import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'

// Mock the api module
vi.mock('@/services/api', () => ({
  api: {
    get: vi.fn().mockRejectedValue(new Error('Network error')),
  },
}))

// Simple wrapper for tests
function TestWrapper({ children }: { children: React.ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{children}</BrowserRouter>
    </QueryClientProvider>
  )
}

describe('App', () => {
  it('should render without crashing', async () => {
    const { default: App } = await import('@/App')
    render(<App />)
    
    // Wait for the lazy-loaded content to appear - check for "Volunteer Portal" text in header
    await waitFor(() => {
      expect(screen.getByText('Volunteer Portal')).toBeInTheDocument()
    }, { timeout: 3000 })
  })
})

describe('NotFoundPage', () => {
  it('should render 404 message', async () => {
    const { default: NotFoundPage } = await import('@/pages/NotFoundPage')
    render(
      <TestWrapper>
        <NotFoundPage />
      </TestWrapper>
    )
    
    expect(screen.getByText('404')).toBeInTheDocument()
    expect(screen.getByText('Page Not Found')).toBeInTheDocument()
    expect(screen.getByText('Go Back Home')).toBeInTheDocument()
  })
})
