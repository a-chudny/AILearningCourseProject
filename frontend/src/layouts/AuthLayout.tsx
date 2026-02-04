import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { Logo } from '@/assets/Logo'

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
            <Logo size={64} />
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
