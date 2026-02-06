import { Link } from 'react-router-dom';
import {
  UserGroupIcon,
  CalendarDaysIcon,
  ClipboardDocumentCheckIcon,
  ChartBarIcon,
  UsersIcon,
  Cog6ToothIcon,
} from '@heroicons/react/24/outline';
import { useAdminStats } from '@/hooks/useAdminStats';

interface StatCardProps {
  title: string;
  value: number | string;
  icon: React.ComponentType<{ className?: string }>;
  loading?: boolean;
}

function StatCard({ title, value, icon: Icon, loading }: StatCardProps) {
  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-sm font-medium text-gray-600">{title}</h3>
          <p className="mt-2 text-3xl font-bold text-gray-900">
            {loading ? (
              <span className="inline-block h-9 w-16 animate-pulse rounded bg-gray-200"></span>
            ) : (
              value
            )}
          </p>
        </div>
        <div className="rounded-full bg-blue-50 p-3">
          <Icon className="h-6 w-6 text-blue-600" />
        </div>
      </div>
    </div>
  );
}

interface QuickActionProps {
  title: string;
  description: string;
  icon: React.ComponentType<{ className?: string }>;
  to: string;
}

function QuickAction({ title, description, icon: Icon, to }: QuickActionProps) {
  return (
    <Link
      to={to}
      className="group flex items-start gap-4 rounded-lg border border-gray-200 bg-white p-6 shadow-sm transition-all hover:border-blue-300 hover:shadow-md"
    >
      <div className="rounded-full bg-blue-50 p-3 transition-colors group-hover:bg-blue-100">
        <Icon className="h-6 w-6 text-blue-600" />
      </div>
      <div className="flex-1">
        <h3 className="text-lg font-semibold text-gray-900 group-hover:text-blue-600">{title}</h3>
        <p className="mt-1 text-sm text-gray-600">{description}</p>
      </div>
    </Link>
  );
}

export default function AdminDashboardPage() {
  const { data: stats, isLoading, isError } = useAdminStats();

  if (isError) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="mt-2 text-gray-600">Welcome to the admin dashboard</p>
        </div>
        <div className="rounded-lg border border-red-200 bg-red-50 p-6">
          <p className="text-red-800">
            Failed to load dashboard statistics. Please try again later.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
        <p className="mt-2 text-gray-600">Welcome to the admin dashboard</p>
      </div>

      {/* Statistics Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title="Total Users"
          value={stats?.totalUsers ?? 0}
          icon={UserGroupIcon}
          loading={isLoading}
        />
        <StatCard
          title="Total Events"
          value={stats?.totalEvents ?? 0}
          icon={CalendarDaysIcon}
          loading={isLoading}
        />
        <StatCard
          title="Total Registrations"
          value={stats?.totalRegistrations ?? 0}
          icon={ClipboardDocumentCheckIcon}
          loading={isLoading}
        />
        <StatCard
          title="Registrations This Month"
          value={stats?.registrationsThisMonth ?? 0}
          icon={ChartBarIcon}
          loading={isLoading}
        />
      </div>

      {/* Quick Actions */}
      <div>
        <h2 className="mb-4 text-xl font-semibold text-gray-900">Quick Actions</h2>
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          <QuickAction
            title="Manage Users"
            description="View, edit, and manage user accounts and roles"
            icon={UsersIcon}
            to="/admin/users"
          />
          <QuickAction
            title="Manage Events"
            description="View, edit, and moderate all events on the platform"
            icon={CalendarDaysIcon}
            to="/admin/events"
          />
          <QuickAction
            title="System Settings"
            description="Configure platform settings and preferences"
            icon={Cog6ToothIcon}
            to="/admin/settings"
          />
        </div>
      </div>
    </div>
  );
}
