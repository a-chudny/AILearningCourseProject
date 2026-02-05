import { useState } from 'react';
import {
  UserGroupIcon,
  CalendarDaysIcon,
  ClipboardDocumentCheckIcon,
  ChartBarIcon,
  ArrowDownTrayIcon,
  CheckCircleIcon,
  XCircleIcon,
} from '@heroicons/react/24/outline';
import {
  exportUsers,
  exportEvents,
  exportRegistrations,
  exportSkillsSummary,
  type RegistrationExportFilters,
} from '@/services/adminService';

interface ExportButtonProps {
  onExport: () => Promise<void>;
  label: string;
  disabled?: boolean;
}

function ExportButton({ onExport, label, disabled }: ExportButtonProps) {
  const [isLoading, setIsLoading] = useState(false);
  const [status, setStatus] = useState<'idle' | 'success' | 'error'>('idle');

  const handleExport = async () => {
    setIsLoading(true);
    setStatus('idle');
    try {
      await onExport();
      setStatus('success');
      setTimeout(() => setStatus('idle'), 3000);
    } catch (error) {
      console.error('Export failed:', error);
      setStatus('error');
      setTimeout(() => setStatus('idle'), 5000);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <button
      onClick={handleExport}
      disabled={isLoading || disabled}
      className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
    >
      {isLoading ? (
        <>
          <svg
            className="h-5 w-5 animate-spin"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            ></circle>
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            ></path>
          </svg>
          <span>Exporting...</span>
        </>
      ) : status === 'success' ? (
        <>
          <CheckCircleIcon className="h-5 w-5" />
          <span>Downloaded!</span>
        </>
      ) : status === 'error' ? (
        <>
          <XCircleIcon className="h-5 w-5" />
          <span>Failed</span>
        </>
      ) : (
        <>
          <ArrowDownTrayIcon className="h-5 w-5" />
          <span>{label}</span>
        </>
      )}
    </button>
  );
}

interface ExportCardProps {
  title: string;
  description: string;
  icon: React.ComponentType<{ className?: string }>;
  onExport: () => Promise<void>;
  children?: React.ReactNode;
}

function ExportCard({ title, description, icon: Icon, onExport, children }: ExportCardProps) {
  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <div className="flex items-start gap-4">
        <div className="rounded-full bg-blue-50 p-3">
          <Icon className="h-6 w-6 text-blue-600" />
        </div>
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
          <p className="mt-1 text-sm text-gray-600">{description}</p>
          {children && <div className="mt-4">{children}</div>}
          <div className="mt-4">
            <ExportButton onExport={onExport} label="Export CSV" />
          </div>
        </div>
      </div>
    </div>
  );
}

/**
 * Helper function to trigger file download from blob
 */
function downloadBlob(blob: Blob, filename: string) {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
}

export default function AdminReportsPage() {
  const [registrationFilters, setRegistrationFilters] = useState<RegistrationExportFilters>({});

  const handleExportUsers = async () => {
    const blob = await exportUsers();
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
    downloadBlob(blob, `users_${timestamp}.csv`);
  };

  const handleExportEvents = async () => {
    const blob = await exportEvents();
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
    downloadBlob(blob, `events_${timestamp}.csv`);
  };

  const handleExportRegistrations = async () => {
    const blob = await exportRegistrations(registrationFilters);
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
    downloadBlob(blob, `registrations_${timestamp}.csv`);
  };

  const handleExportSkills = async () => {
    const blob = await exportSkillsSummary();
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
    downloadBlob(blob, `skills_summary_${timestamp}.csv`);
  };

  const handleStartDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRegistrationFilters((prev) => ({
      ...prev,
      startDate: e.target.value || undefined,
    }));
  };

  const handleEndDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRegistrationFilters((prev) => ({
      ...prev,
      endDate: e.target.value || undefined,
    }));
  };

  const handleClearFilters = () => {
    setRegistrationFilters({});
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Reports & Exports</h1>
        <p className="mt-2 text-gray-600">
          Export platform data to CSV files for analysis and reporting
        </p>
      </div>

      {/* Export Cards */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Users Export */}
        <ExportCard
          title="Users Export"
          description="Export all users with their email, role, skills, and registration date. Includes both volunteers and organizers."
          icon={UserGroupIcon}
          onExport={handleExportUsers}
        />

        {/* Events Export */}
        <ExportCard
          title="Events Export"
          description="Export all events including title, date, location, capacity, current registrations, and organizer information."
          icon={CalendarDaysIcon}
          onExport={handleExportEvents}
        />

        {/* Registrations Export with Date Filters */}
        <ExportCard
          title="Registrations Export"
          description="Export all event registrations with volunteer details, event information, status, and registration date. Filter by date range."
          icon={ClipboardDocumentCheckIcon}
          onExport={handleExportRegistrations}
        >
          <div className="space-y-3 rounded-lg border border-gray-200 bg-gray-50 p-4">
            <h4 className="text-sm font-semibold text-gray-700">Date Filters (Optional)</h4>
            <div className="grid gap-3 sm:grid-cols-2">
              <div>
                <label htmlFor="startDate" className="block text-xs font-medium text-gray-700">
                  Start Date
                </label>
                <input
                  type="date"
                  id="startDate"
                  value={registrationFilters.startDate || ''}
                  onChange={handleStartDateChange}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label htmlFor="endDate" className="block text-xs font-medium text-gray-700">
                  End Date
                </label>
                <input
                  type="date"
                  id="endDate"
                  value={registrationFilters.endDate || ''}
                  onChange={handleEndDateChange}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            </div>
            {(registrationFilters.startDate || registrationFilters.endDate) && (
              <button
                onClick={handleClearFilters}
                className="text-xs text-blue-600 hover:text-blue-700"
              >
                Clear filters
              </button>
            )}
          </div>
        </ExportCard>

        {/* Skills Summary Export */}
        <ExportCard
          title="Skills Summary Export"
          description="Export skills statistics including skill name, category, number of volunteers with each skill, and related event count."
          icon={ChartBarIcon}
          onExport={handleExportSkills}
        />
      </div>

      {/* Information Section */}
      <div className="rounded-lg border border-blue-200 bg-blue-50 p-6">
        <h3 className="text-sm font-semibold text-blue-900">About Exports</h3>
        <ul className="mt-2 space-y-1 text-sm text-blue-800">
          <li>• All exports are in CSV format and can be opened in Excel or any spreadsheet software</li>
          <li>• Files are timestamped with the export date and time</li>
          <li>• Date filters for registrations are based on registration date, not event date</li>
          <li>• Exports include all data visible to admin users, including soft-deleted records</li>
        </ul>
      </div>
    </div>
  );
}
