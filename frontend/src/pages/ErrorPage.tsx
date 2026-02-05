import { Link, useRouteError, isRouteErrorResponse } from 'react-router-dom'

interface ErrorPageProps {
  /** Custom error to display (for non-route errors) */
  error?: Error | null
  /** Custom title */
  title?: string
  /** Custom message */
  message?: string
}

/**
 * Generic error page for displaying errors.
 * Can be used as a route error element or standalone error page.
 */
export default function ErrorPage({ error, title, message }: ErrorPageProps) {
  const routeError = useRouteError()

  // Determine error details
  let errorTitle = title || 'Something went wrong'
  let errorMessage =
    message || 'We apologize for the inconvenience. Please try again later.'
  let statusCode: number | null = null

  if (isRouteErrorResponse(routeError)) {
    statusCode = routeError.status
    if (routeError.status === 404) {
      errorTitle = 'Page Not Found'
      errorMessage = "Sorry, the page you're looking for doesn't exist."
    } else if (routeError.status === 403) {
      errorTitle = 'Access Denied'
      errorMessage = "You don't have permission to view this page."
    } else if (routeError.status === 401) {
      errorTitle = 'Unauthorized'
      errorMessage = 'Please log in to access this page.'
    } else if (routeError.status >= 500) {
      errorTitle = 'Server Error'
      errorMessage = "We're experiencing technical difficulties. Please try again later."
    }
  } else if (routeError instanceof Error) {
    errorMessage = routeError.message
  } else if (error) {
    errorMessage = error.message
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gray-50 px-4">
      <div className="max-w-md text-center">
        {/* Status code badge */}
        {statusCode && (
          <span className="mb-4 inline-block rounded-full bg-red-100 px-4 py-1 text-sm font-medium text-red-800">
            Error {statusCode}
          </span>
        )}

        {/* Error icon */}
        <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-red-100">
          {statusCode === 404 ? (
            <svg
              className="h-10 w-10 text-red-600"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
          ) : statusCode === 403 || statusCode === 401 ? (
            <svg
              className="h-10 w-10 text-red-600"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
              />
            </svg>
          ) : (
            <svg
              className="h-10 w-10 text-red-600"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
              />
            </svg>
          )}
        </div>

        <h1 className="mb-2 text-3xl font-bold text-gray-900">{errorTitle}</h1>
        <p className="mb-8 text-gray-600">{errorMessage}</p>

        {/* Error details in development */}
        {import.meta.env.DEV && (routeError || error) && (
          <div className="mb-6 rounded-lg bg-gray-100 p-4 text-left">
            <p className="font-mono text-xs text-gray-700">
              {routeError instanceof Error
                ? routeError.stack || routeError.message
                : error?.stack || error?.message || JSON.stringify(routeError)}
            </p>
          </div>
        )}

        {/* Action buttons */}
        <div className="flex flex-col gap-3 sm:flex-row sm:justify-center">
          <button
            onClick={() => window.history.back()}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-gray-300 bg-white px-6 py-3 font-medium text-gray-700 transition-colors hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          >
            <svg
              className="h-5 w-5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M10 19l-7-7m0 0l7-7m-7 7h18"
              />
            </svg>
            Go Back
          </button>
          <Link
            to="/"
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-6 py-3 font-medium text-white transition-colors hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          >
            <svg
              className="h-5 w-5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
              />
            </svg>
            Go Home
          </Link>
          {statusCode === 401 && (
            <Link
              to="/login"
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-green-600 px-6 py-3 font-medium text-white transition-colors hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2"
            >
              Log In
            </Link>
          )}
        </div>
      </div>
    </div>
  )
}
