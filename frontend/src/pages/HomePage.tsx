import { useQuery } from '@tanstack/react-query'
import { api } from '@/services/api'

interface HealthCheckResponse {
  status: string
  timestamp: string
}

async function fetchHealthCheck(): Promise<HealthCheckResponse> {
  const response = await api.get<HealthCheckResponse>('/health')
  return response.data
}

export default function HomePage() {
  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['health'],
    queryFn: fetchHealthCheck,
    retry: false,
  })

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-16">
        <header className="text-center">
          <h1 className="mb-4 text-5xl font-bold text-gray-900">Volunteer Event Portal</h1>
          <p className="mb-8 text-xl text-gray-600">
            Connect volunteers with meaningful community events
          </p>
        </header>

        <main className="mx-auto max-w-2xl">
          {/* API Health Check Card */}
          <div className="rounded-xl bg-white p-8 shadow-lg">
            <h2 className="mb-4 text-2xl font-semibold text-gray-800">Backend Connection Status</h2>

            {isLoading && (
              <div className="flex items-center gap-3 text-blue-600">
                <div className="h-5 w-5 animate-spin rounded-full border-2 border-blue-600 border-t-transparent" />
                <span>Checking connection...</span>
              </div>
            )}

            {isError && (
              <div className="rounded-lg bg-red-50 p-4 text-red-700">
                <p className="font-medium">Connection Failed</p>
                <p className="text-sm">
                  {error instanceof Error ? error.message : 'Unable to connect to backend'}
                </p>
                <p className="mt-2 text-xs text-red-600">
                  Make sure the backend is running on http://localhost:5000
                </p>
              </div>
            )}

            {data && (
              <div className="rounded-lg bg-green-50 p-4 text-green-700">
                <p className="font-medium">Connected Successfully!</p>
                <p className="text-sm">Status: {data.status}</p>
                <p className="text-xs text-green-600">
                  Last checked: {new Date(data.timestamp).toLocaleString()}
                </p>
              </div>
            )}
          </div>

          {/* Feature Cards */}
          <div className="mt-8 grid gap-6 md:grid-cols-2">
            <div className="rounded-xl bg-white p-6 shadow-md transition-shadow hover:shadow-lg">
              <div className="mb-3 text-3xl"></div>
              <h3 className="mb-2 text-lg font-semibold text-gray-800">Browse Events</h3>
              <p className="text-gray-600">Discover volunteer opportunities in your community</p>
            </div>

            <div className="rounded-xl bg-white p-6 shadow-md transition-shadow hover:shadow-lg">
              <div className="mb-3 text-3xl"></div>
              <h3 className="mb-2 text-lg font-semibold text-gray-800">Sign Up</h3>
              <p className="text-gray-600">Register for events that match your interests</p>
            </div>

          </div>
        </main>
      </div>
    </div>
  )
}
