import { useLocation } from 'react-router-dom'
import { useEffect, useState } from 'react'

export default function MyEventsPage() {
  const location = useLocation()
  const [successMessage, setSuccessMessage] = useState<string>('')

  useEffect(() => {
    // Check for success message from navigation state
    if (location.state?.message) {
      setSuccessMessage(location.state.message)
      
      // Clear message after 5 seconds
      const timer = setTimeout(() => {
        setSuccessMessage('')
      }, 5000)

      return () => clearTimeout(timer)
    }
  }, [location.state])

  return (
    <div className="max-w-7xl mx-auto">
      {/* Success Message */}
      {successMessage && (
        <div className="mb-6 rounded-md bg-green-50 p-4">
          <div className="flex">
            <svg
              className="h-5 w-5 text-green-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <p className="ml-3 text-sm font-medium text-green-800">{successMessage}</p>
          </div>
        </div>
      )}

      {/* Page Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">My Events</h1>
        <p className="mt-2 text-sm text-gray-600">
          Manage your created events and view your registrations
        </p>
      </div>

      {/* Placeholder Content */}
      <div className="bg-white rounded-lg shadow-md p-8 text-center">
        <svg
          className="mx-auto h-24 w-24 text-gray-400"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
          />
        </svg>
        <h2 className="mt-4 text-xl font-semibold text-gray-900">My Events Page</h2>
        <p className="mt-2 text-gray-600">
          This page is under construction and will be implemented in a future story.
        </p>
        <p className="mt-4 text-sm text-gray-500">
          You will be able to:
        </p>
        <ul className="mt-2 text-sm text-gray-500 space-y-1">
          <li> View events you've created</li>
          <li> Manage your event registrations</li>
          <li> Edit and cancel events</li>
          <li> Track volunteer sign-ups</li>
        </ul>
      </div>
    </div>
  )
}
