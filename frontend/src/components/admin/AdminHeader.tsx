import { Link } from 'react-router-dom';
import { ArrowLeftIcon, UserCircleIcon } from '@heroicons/react/24/outline';
import { useAuth } from '@/hooks/useAuth';
import { UserRoleLabels } from '@/types/enums';

export function AdminHeader() {
  const { user } = useAuth();

  return (
    <header className="sticky top-0 z-40 flex h-16 items-center justify-between border-b border-gray-200 bg-white px-4 md:px-6 shadow-sm">
      {/* Left: Back to Main Site */}
      <Link
        to="/"
        className="flex items-center gap-2 rounded-lg px-3 py-2 text-gray-700 hover:bg-gray-100 transition-colors"
      >
        <ArrowLeftIcon className="h-5 w-5" />
        <span className="font-medium">Back to Main Site</span>
      </Link>

      {/* Right: User Info */}
      <div className="flex items-center gap-3">
        <div className="flex items-center gap-2 text-sm">
          <UserCircleIcon className="h-6 w-6 text-gray-500" />
          <div className="hidden sm:block">
            <div className="font-medium text-gray-900">{user?.name}</div>
            <div className="text-xs text-gray-500">
              {user?.role !== undefined && UserRoleLabels[user.role]}
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}
