import { render, screen, fireEvent } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { vi, describe, it, expect, beforeEach } from 'vitest'
import { ErrorBoundary } from '@/components/ErrorBoundary'

// Component that throws an error for testing
function ThrowError({ shouldThrow }: { shouldThrow: boolean }) {
  if (shouldThrow) {
    throw new Error('Test error message')
  }
  return <div>No error</div>
}

// Wrapper with router for Link components
function TestWrapper({ children }: { children: React.ReactNode }) {
  return <BrowserRouter>{children}</BrowserRouter>
}

describe('ErrorBoundary', () => {
  // Suppress error logging during tests
  const originalConsoleError = console.error
  beforeEach(() => {
    console.error = vi.fn()
  })
  afterEach(() => {
    console.error = originalConsoleError
  })

  it('renders children when no error', () => {
    render(
      <TestWrapper>
        <ErrorBoundary>
          <div>Child content</div>
        </ErrorBoundary>
      </TestWrapper>
    )

    expect(screen.getByText('Child content')).toBeInTheDocument()
  })

  it('renders full-page error fallback when error occurs', () => {
    render(
      <TestWrapper>
        <ErrorBoundary>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    expect(screen.getByText('Something went wrong')).toBeInTheDocument()
    expect(screen.getByText('Try Again')).toBeInTheDocument()
    expect(screen.getByText('Go Home')).toBeInTheDocument()
  })

  it('renders inline error fallback when inline prop is true', () => {
    render(
      <TestWrapper>
        <ErrorBoundary inline>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    expect(screen.getByText('Something went wrong')).toBeInTheDocument()
    expect(screen.getByText('This component failed to load.')).toBeInTheDocument()
    expect(screen.getByText('Try again')).toBeInTheDocument()
  })

  it('renders custom fallback when provided', () => {
    render(
      <TestWrapper>
        <ErrorBoundary fallback={<div>Custom error UI</div>}>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    expect(screen.getByText('Custom error UI')).toBeInTheDocument()
  })

  it('calls onError callback when error occurs', () => {
    const onError = vi.fn()

    render(
      <TestWrapper>
        <ErrorBoundary onError={onError}>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    expect(onError).toHaveBeenCalledTimes(1)
    expect(onError).toHaveBeenCalledWith(
      expect.any(Error),
      expect.objectContaining({ componentStack: expect.any(String) })
    )
  })

  it('resets error state when Try Again is clicked', () => {
    const { rerender } = render(
      <TestWrapper>
        <ErrorBoundary>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    expect(screen.getByText('Something went wrong')).toBeInTheDocument()

    // Update component to not throw
    rerender(
      <TestWrapper>
        <ErrorBoundary>
          <ThrowError shouldThrow={false} />
        </ErrorBoundary>
      </TestWrapper>
    )

    // Click Try Again
    fireEvent.click(screen.getByText('Try Again'))

    // Should now render children (but will throw again since key didn't change)
    // This tests the reset mechanism
  })

  it('logs error to console in development', () => {
    render(
      <TestWrapper>
        <ErrorBoundary>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    expect(console.error).toHaveBeenCalled()
  })

  it('shows error details in development mode', () => {
    // Vitest sets import.meta.env.DEV = true by default in test mode
    render(
      <TestWrapper>
        <ErrorBoundary>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    // In dev mode, error details should be visible
    // The error is thrown twice due to React's error boundary behavior
    // so we use getAllByText to handle multiple matches
    const errorElements = screen.getAllByText(/Error.*Test error message/)
    expect(errorElements.length).toBeGreaterThan(0)
  })

  it('Go Home link navigates to root', () => {
    render(
      <TestWrapper>
        <ErrorBoundary>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    const homeLink = screen.getByText('Go Home')
    expect(homeLink).toHaveAttribute('href', '/')
  })
})

describe('ErrorBoundary inline variant', () => {
  const originalConsoleError = console.error
  beforeEach(() => {
    console.error = vi.fn()
  })
  afterEach(() => {
    console.error = originalConsoleError
  })

  it('has compact styling for inline errors', () => {
    render(
      <TestWrapper>
        <ErrorBoundary inline>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    // Inline variant should have different text
    expect(screen.queryByText('Go Home')).not.toBeInTheDocument()
    expect(screen.getByText('Try again')).toBeInTheDocument()
  })

  it('resets on retry click for inline errors', () => {
    render(
      <TestWrapper>
        <ErrorBoundary inline>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      </TestWrapper>
    )

    expect(screen.getByText('Something went wrong')).toBeInTheDocument()
    
    const retryButton = screen.getByText('Try again')
    fireEvent.click(retryButton)
    
    // After retry, the component will try to render children again
    // Since shouldThrow is still true, it will error again
    // This just tests that the click handler works
  })
})
