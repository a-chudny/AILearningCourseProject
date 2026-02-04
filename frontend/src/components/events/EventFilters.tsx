import { useState, useEffect } from 'react';
import { EventStatus } from '@/types/enums';

interface EventFiltersProps {
  onFilterChange: (filters: {
    searchTerm?: string;
    status?: string;
    includePastEvents?: boolean;
  }) => void;
  initialSearchTerm?: string;
  initialStatus?: string;
  initialIncludePastEvents?: boolean;
}

export function EventFilters({
  onFilterChange,
  initialSearchTerm = '',
  initialStatus = '',
  initialIncludePastEvents = false,
}: EventFiltersProps) {
  const [searchTerm, setSearchTerm] = useState(initialSearchTerm);
  const [status, setStatus] = useState(initialStatus);
  const [includePastEvents, setIncludePastEvents] = useState(initialIncludePastEvents);

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      onFilterChange({
        searchTerm: searchTerm || undefined,
        status: status || undefined,
        includePastEvents,
      });
    }, 300); // 300ms debounce

    return () => clearTimeout(timer);
  }, [searchTerm, status, includePastEvents, onFilterChange]);

  const handleReset = () => {
    setSearchTerm('');
    setStatus('');
    setIncludePastEvents(false);
  };

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {/* Search input */}
        <div className="lg:col-span-2">
          <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-1">
            Search Events
          </label>
          <input
            id="search"
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search by title or description..."
            className="w-full rounded-lg border border-gray-300 px-4 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Status filter */}
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
            Event Status
          </label>
          <select
            id="status"
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            className="w-full rounded-lg border border-gray-300 px-4 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Statuses</option>
            <option value={EventStatus.Active}>Active</option>
            <option value={EventStatus.Cancelled}>Cancelled</option>
          </select>
        </div>

        {/* Include past events checkbox */}
        <div className="flex items-end">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              checked={includePastEvents}
              onChange={(e) => setIncludePastEvents(e.target.checked)}
              className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-2 focus:ring-blue-500"
            />
            <span className="text-sm font-medium text-gray-700">Include past events</span>
          </label>
        </div>
      </div>

      {/* Reset button */}
      {(searchTerm || status || includePastEvents) && (
        <div className="mt-4 flex justify-end">
          <button
            onClick={handleReset}
            className="text-sm font-medium text-blue-600 hover:text-blue-700 transition-colors"
          >
            Reset Filters
          </button>
        </div>
      )}
    </div>
  );
}
