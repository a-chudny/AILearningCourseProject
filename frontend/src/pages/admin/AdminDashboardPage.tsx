export default function AdminDashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
        <p className="mt-2 text-gray-600">Welcome to the admin dashboard</p>
      </div>

      {/* Placeholder content - will be implemented in ADM-002 */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        {['Total Users', 'Total Events', 'Total Registrations', 'Upcoming Events'].map((title) => (
          <div key={title} className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
            <h3 className="text-sm font-medium text-gray-600">{title}</h3>
            <p className="mt-2 text-3xl font-bold text-gray-900">-</p>
            <p className="mt-1 text-sm text-gray-500">Loading...</p>
          </div>
        ))}
      </div>

      <div className="rounded-lg border border-gray-200 bg-white p-6">
        <h2 className="text-xl font-semibold text-gray-900">Quick Actions</h2>
        <p className="mt-2 text-gray-600">
          Admin dashboard functionality will be fully implemented in ADM-002
        </p>
      </div>
    </div>
  )
}