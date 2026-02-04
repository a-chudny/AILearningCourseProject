import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'

interface AuthLayoutProps {
  children: ReactNode
}

export default function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-12 sm:px-6 lg:px-8">
      <div className="w-full max-w-md">
        {/* Header with Logo and Title */}
        <div className="text-center">
          {/* Simple SVG Logo */}
          <div className="flex justify-center mb-4">
            <svg
              width="64"
              height="64"
              viewBox="0 0 64 64"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
              className="text-blue-600"
              aria-label="Volunteer Event Portal Logo"
            >
              {/* Heart shape representing volunteering/care */}
              <path
                d="M32 54C32 54 8 38 8 22C8 16.4 12.4 12 18 12C22 12 25.5 14.5 27 18C27 18 27 18 27 18C28.5 14.5 32 12 36 12C41.6 12 46 16.4 46 22C46 38 32 54 32 54Z"
                fill="currentColor"
                className="text-blue-600"
              />
              {/* Hands joining/helping symbol */}
              <path
                d="M20 28L24 32L28 28M36 28L40 32L44 28"
                stroke="white"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
              {/* Circle border */}
              <circle
                cx="32"
                cy="32"
                r="30"
                stroke="currentColor"
                strokeWidth="2"
                className="text-blue-600"
              />
            </svg>
          </div>

          {/* App Title */}
          <h1 className="text-3xl font-bold tracking-tight text-gray-900">
            Volunteer Event Portal
          </h1>

          {/* Back to Home Link */}
          <div className="mt-4">
            <Link
              to="/"
              className="inline-flex items-center gap-2 text-sm font-medium text-blue-600 hover:text-blue-500 focus:outline-none focus:underline"
            >
              <svg
                className="h-4 w-4"
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
              Back to Home
            </Link>
          </div>
        </div>

        {/* Content Card */}
        <div className="mt-8 bg-white px-6 py-8 shadow-md rounded-lg sm:px-10">
          {children}
        </div>
      </div>
    </div>
  )
}
