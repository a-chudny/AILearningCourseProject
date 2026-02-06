import { Link, useLocation } from 'react-router-dom';
import {
  HomeIcon,
  UsersIcon,
  CalendarIcon,
  DocumentChartBarIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
} from '@heroicons/react/24/outline';

interface AdminSidebarProps {
  isCollapsed: boolean;
  onToggleCollapse: () => void;
}

const navigationLinks = [
  { to: '/admin', label: 'Dashboard', icon: HomeIcon },
  { to: '/admin/users', label: 'Users', icon: UsersIcon },
  { to: '/admin/events', label: 'Events', icon: CalendarIcon },
  { to: '/admin/reports', label: 'Reports', icon: DocumentChartBarIcon },
];

export function AdminSidebar({ isCollapsed, onToggleCollapse }: AdminSidebarProps) {
  const location = useLocation();

  const isActive = (path: string) => {
    if (path === '/admin') {
      return location.pathname === path;
    }
    return location.pathname.startsWith(path);
  };

  return (
    <>
      {/* Desktop Sidebar */}
      <aside
        className={`hidden md:flex fixed left-0 top-0 h-screen flex-col bg-gray-800 text-white transition-all duration-300 ${
          isCollapsed ? 'w-16' : 'w-64'
        }`}
      >
        {/* Logo/Brand */}
        <div className="flex h-16 items-center justify-between border-b border-gray-700 px-4">
          {!isCollapsed && (
            <Link to="/admin" className="text-xl font-bold text-white hover:text-gray-200">
              Admin Panel
            </Link>
          )}
          <button
            onClick={onToggleCollapse}
            className="rounded-lg p-2 hover:bg-gray-700 transition-colors"
            aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          >
            {isCollapsed ? (
              <ChevronRightIcon className="h-5 w-5" />
            ) : (
              <ChevronLeftIcon className="h-5 w-5" />
            )}
          </button>
        </div>

        {/* Navigation Links */}
        <nav className="flex-1 space-y-1 px-3 py-4">
          {navigationLinks.map((link) => {
            const Icon = link.icon;
            const active = isActive(link.to);

            return (
              <Link
                key={link.to}
                to={link.to}
                className={`flex items-center gap-3 rounded-lg px-3 py-2.5 transition-colors ${
                  active
                    ? 'bg-blue-600 text-white'
                    : 'text-gray-300 hover:bg-gray-700 hover:text-white'
                }`}
                title={isCollapsed ? link.label : undefined}
              >
                <Icon className="h-6 w-6 flex-shrink-0" />
                {!isCollapsed && <span className="font-medium">{link.label}</span>}
              </Link>
            );
          })}
        </nav>
      </aside>

      {/* Mobile Bottom Navigation */}
      <nav className="md:hidden fixed bottom-0 left-0 right-0 z-50 bg-gray-800 border-t border-gray-700">
        <div className="grid grid-cols-4 gap-1 px-2 py-2">
          {navigationLinks.map((link) => {
            const Icon = link.icon;
            const active = isActive(link.to);

            return (
              <Link
                key={link.to}
                to={link.to}
                className={`flex flex-col items-center gap-1 rounded-lg px-2 py-2 transition-colors ${
                  active
                    ? 'bg-blue-600 text-white'
                    : 'text-gray-300 hover:bg-gray-700 hover:text-white'
                }`}
              >
                <Icon className="h-6 w-6" />
                <span className="text-xs font-medium">{link.label}</span>
              </Link>
            );
          })}
        </div>
      </nav>
    </>
  );
}
