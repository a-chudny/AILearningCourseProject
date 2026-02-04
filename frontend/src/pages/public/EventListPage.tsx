import { useState } from 'react';
import { useEvents } from '@/hooks/useEvents';
import { EventCard } from '@/components/events/EventCard';
import { Pagination } from '@/components/events/Pagination';
import { EventFilters } from '@/components/events/EventFilters';
import type { EventQueryParams } from '@/types/api';

export default function EventListPage() {
  // State for query parameters
  const [queryParams, setQueryParams] = useState<EventQueryParams>({
    page: 1,
    pageSize: 20,
    includePastEvents: false,
    sortBy: 'StartTime',
    sortDirection: 'asc',
  });

  // Fetch events using React Query
  const { data, isLoading, isError, error } = useEvents(queryParams);

  // Handle page change
  const handlePageChange = (page: number) => {
    setQueryParams((prev) => ({ ...prev, page }));
    // Scroll to top of page
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Handle filter changes
  const handleFilterChange = (filters: {
    searchTerm?: string;
    status?: string;
    includePastEvents?: boolean;
  }) => {
    setQueryParams((prev) => ({
      ...prev,
      ...filters,
      page: 1, // Reset to first page when filters change
    }));
  };

  // Handle page size change
  const handlePageSizeChange = (pageSize: number) => {
    setQueryParams((prev) => ({
      ...prev,
      pageSize,
      page: 1, // Reset to first page when page size changes
    }));
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        {/* Page header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Volunteer Events</h1>
          <p className="mt-2 text-gray-600">
            Discover opportunities to make a difference in your community
          </p>
        </div>

        {/* Filters */}
        <div className="mb-6">
          <EventFilters
            onFilterChange={handleFilterChange}
            initialSearchTerm={queryParams.searchTerm}
            initialStatus={queryParams.status}
            initialIncludePastEvents={queryParams.includePastEvents}
          />
        </div>

        {/* Page size selector */}
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <label htmlFor="pageSize" className="text-sm font-medium text-gray-700">
              Events per page:
            </label>
            <select
              id="pageSize"
              value={queryParams.pageSize}
              onChange={(e) => handlePageSizeChange(Number(e.target.value))}
              className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
              <option value={100}>100</option>
            </select>
          </div>

          {/* Results count */}
          {data && (
            <div className="text-sm text-gray-600">
              Showing {data.events.length > 0 ? (data.page - 1) * data.pageSize + 1 : 0} -{' '}
              {Math.min(data.page * data.pageSize, data.totalCount)} of {data.totalCount} events
            </div>
          )}
        </div>

        {/* Loading state */}
        {isLoading && (
          <div className="flex flex-col items-center justify-center py-16">
            <div className="h-12 w-12 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
            <p className="mt-4 text-gray-600">Loading events...</p>
          </div>
        )}

        {/* Error state */}
        {isError && (
          <div className="rounded-lg border border-red-200 bg-red-50 p-6 text-center">
            <svg
              className="mx-auto h-12 w-12 text-red-500"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <h3 className="mt-4 text-lg font-semibold text-red-900">Failed to load events</h3>
            <p className="mt-2 text-sm text-red-700">
              {error instanceof Error ? error.message : 'An unexpected error occurred'}
            </p>
            <button
              onClick={() => window.location.reload()}
              className="mt-4 rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-red-700"
            >
              Retry
            </button>
          </div>
        )}

        {/* Empty state */}
        {!isLoading && !isError && data && data.events.length === 0 && (
          <div className="rounded-lg border border-gray-200 bg-white p-12 text-center">
            <svg
              className="mx-auto h-16 w-16 text-gray-400"
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
            <h3 className="mt-4 text-lg font-semibold text-gray-900">No events found</h3>
            <p className="mt-2 text-gray-600">
              {queryParams.searchTerm || queryParams.status
                ? 'Try adjusting your filters to see more results'
                : 'Check back later for upcoming volunteer opportunities'}
            </p>
          </div>
        )}

        {/* Events grid */}
        {!isLoading && !isError && data && data.events.length > 0 && (
          <>
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {data.events.map((event) => (
                <EventCard key={event.id} event={event} />
              ))}
            </div>

            {/* Pagination */}
            <div className="mt-8">
              <Pagination
                currentPage={data.page}
                totalPages={data.totalPages}
                onPageChange={handlePageChange}
                hasPreviousPage={data.hasPreviousPage}
                hasNextPage={data.hasNextPage}
              />
            </div>
          </>
        )}
      </div>
    </div>
  );
}
