import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { MagnifyingGlassIcon, FunnelIcon, PencilIcon, TrashIcon, XCircleIcon, EyeIcon } from '@heroicons/react/24/outline';
import { useAdminEvents, useSoftDeleteEvent, useCancelEventMutation, useEventRegistrations } from '@/hooks/useAdminEvents';
import { toast } from '@/utils/toast';
import type { AdminEventsQueryParams } from '@/services/adminService';

// Debounce hook
function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

interface DeleteConfirmModalProps {
  event: any | null;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isLoading: boolean;
}

function DeleteConfirmModal({ event, isOpen, onClose, onConfirm, isLoading }: DeleteConfirmModalProps) {
  if (!isOpen || !event) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
      <div className="w-full max-w-md rounded-lg bg-white shadow-xl">
        <div className="border-b border-gray-200 px-6 py-4">
          <h2 className="text-xl font-bold text-red-600">Delete Event</h2>
          <p className="mt-1 text-sm text-gray-600">This action will hide the event</p>
        </div>

        <div className="px-6 py-4">
          <p className="text-gray-700">
            Are you sure you want to delete <strong>{event.title}</strong>?
          </p>
          <p className="mt-2 text-sm text-gray-500">
            The event will be soft-deleted and hidden from public view. This action can be reversed by updating the database directly.
          </p>
        </div>

        <div className="flex justify-end gap-3 border-t border-gray-200 px-6 py-4">
          <button
            type="button"
            onClick={onClose}
            disabled={isLoading}
            className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={isLoading}
            className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
          >
            {isLoading ? 'Deleting...' : 'Delete Event'}
          </button>
        </div>
      </div>
    </div>
  );
}

interface CancelConfirmModalProps {
  event: any | null;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isLoading: boolean;
}

function CancelConfirmModal({ event, isOpen, onClose, onConfirm, isLoading }: CancelConfirmModalProps) {
  if (!isOpen || !event) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
      <div className="w-full max-w-md rounded-lg bg-white shadow-xl">
        <div className="border-b border-gray-200 px-6 py-4">
          <h2 className="text-xl font-bold text-amber-600">Cancel Event</h2>
          <p className="mt-1 text-sm text-gray-600">This action will prevent new registrations</p>
        </div>

        <div className="px-6 py-4">
          <p className="text-gray-700">
            Are you sure you want to cancel <strong>{event.title}</strong>?
          </p>
          <p className="mt-2 text-sm text-gray-500">
            The event will be marked as cancelled and new registrations will be prevented.
          </p>
        </div>

        <div className="flex justify-end gap-3 border-t border-gray-200 px-6 py-4">
          <button
            type="button"
            onClick={onClose}
            disabled={isLoading}
            className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
          >
            Keep Active
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={isLoading}
            className="rounded-md bg-amber-600 px-4 py-2 text-sm font-medium text-white hover:bg-amber-700 disabled:opacity-50"
          >
            {isLoading ? 'Cancelling...' : 'Cancel Event'}
          </button>
        </div>
      </div>
    </div>
  );
}

interface RegistrationsModalProps {
  eventId: number | null;
  eventTitle: string;
  isOpen: boolean;
  onClose: () => void;
}

function RegistrationsModal({ eventId, eventTitle, isOpen, onClose }: RegistrationsModalProps) {
  const { data: registrations, isLoading } = useEventRegistrations(eventId);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
      <div className="w-full max-w-3xl rounded-lg bg-white shadow-xl max-h-[90vh] overflow-hidden flex flex-col">
        <div className="border-b border-gray-200 px-6 py-4">
          <h2 className="text-xl font-bold text-gray-900">Event Registrations</h2>
          <p className="mt-1 text-sm text-gray-600">{eventTitle}</p>
        </div>

        <div className="flex-1 overflow-y-auto px-6 py-4">
          {isLoading ? (
            <div className="flex justify-center py-8">
              <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-500 border-t-transparent"></div>
            </div>
          ) : !registrations || registrations.length === 0 ? (
            <p className="text-center text-gray-500 py-8">No registrations for this event yet.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Name</th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Email</th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Phone</th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Status</th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Registered</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 bg-white">
                  {registrations.map((reg: any) => (
                    <tr key={reg.id}>
                      <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">{reg.user.name}</td>
                      <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">{reg.user.email}</td>
                      <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">{reg.user.phoneNumber || '-'}</td>
                      <td className="whitespace-nowrap px-6 py-4">
                        <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${
                          reg.status === 0 ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'
                        }`}>
                          {reg.status === 0 ? 'Confirmed' : 'Cancelled'}
                        </span>
                      </td>
                      <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                        {new Date(reg.registeredAt).toLocaleDateString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <div className="flex justify-end border-t border-gray-200 px-6 py-4">
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
}

export default function AdminEventsPage() {
  const navigate = useNavigate();
  const [searchInput, setSearchInput] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const debouncedSearch = useDebounce(searchInput, 300);

  // Build query params
  const queryParams: AdminEventsQueryParams = {
    page,
    pageSize,
    searchTerm: debouncedSearch || undefined,
    includePastEvents: true,
    includeDeleted: true,
    status: statusFilter === 'active' ? 0 : statusFilter === 'cancelled' ? 1 : null,
  };

  const { data, isLoading, isError, refetch } = useAdminEvents(queryParams);
  const deleteMutation = useSoftDeleteEvent();
  const cancelMutation = useCancelEventMutation();

  const [deleteModalEvent, setDeleteModalEvent] = useState<any | null>(null);
  const [cancelModalEvent, setCancelModalEvent] = useState<any | null>(null);
  const [registrationsModalEvent, setRegistrationsModalEvent] = useState<{ id: number; title: string } | null>(null);

  useEffect(() => {
    setPage(1);
  }, [debouncedSearch, statusFilter]);

  const handleDelete = async () => {
    if (!deleteModalEvent) return;

    try {
      await deleteMutation.mutateAsync(deleteModalEvent.id);
      toast.success(`Event "${deleteModalEvent.title}" has been deleted`);
      setDeleteModalEvent(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to delete event';
      toast.error(message);
    }
  };

  const handleCancel = async () => {
    if (!cancelModalEvent) return;

    try {
      await cancelMutation.mutateAsync(cancelModalEvent.id);
      toast.success(`Event "${cancelModalEvent.title}" has been cancelled`);
      setCancelModalEvent(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to cancel event';
      toast.error(message);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const calculateEndTime = (startTime: string, durationMinutes: number) => {
    const start = new Date(startTime);
    const end = new Date(start.getTime() + durationMinutes * 60000);
    return end.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusBadgeColor = (status: number) => {
    return status === 0 ? 'bg-green-100 text-green-800' : 'bg-amber-100 text-amber-800';
  };

  if (isError) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Event Management</h1>
          <p className="mt-2 text-gray-600">View and manage all events</p>
        </div>
        <div className="rounded-lg border border-red-200 bg-red-50 p-6">
          <p className="text-red-800">Failed to load events. Please try again later.</p>
          <button
            onClick={() => refetch()}
            className="mt-4 rounded-md bg-red-600 px-4 py-2 text-white hover:bg-red-700"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Event Management</h1>
        <p className="mt-2 text-gray-600">View and manage all platform events</p>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by title..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 text-gray-900 bg-white focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>

        <div className="flex items-center gap-2">
          <FunnelIcon className="h-5 w-5 text-gray-500" />
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="rounded-md border border-gray-300 px-3 py-2 text-gray-900 bg-white focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            <option value="all">All Events</option>
            <option value="active">Active Only</option>
            <option value="cancelled">Cancelled Only</option>
          </select>
        </div>
      </div>

      <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-1/4">Title</th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-1/6">Organizer</th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-1/5">Date/Time</th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-24">Registrations</th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-24">Status</th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500 w-40">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {isLoading ? (
                Array.from({ length: 5 }).map((_, index) => (
                  <tr key={index}>
                    <td className="px-6 py-4">
                      <div className="h-4 w-full max-w-48 animate-pulse rounded bg-gray-200" />
                    </td>
                    <td className="px-6 py-4">
                      <div className="h-4 w-full max-w-32 animate-pulse rounded bg-gray-200" />
                    </td>
                    <td className="px-6 py-4">
                      <div className="h-4 w-full max-w-40 animate-pulse rounded bg-gray-200" />
                    </td>
                    <td className="px-6 py-4">
                      <div className="h-4 w-16 animate-pulse rounded bg-gray-200" />
                    </td>
                    <td className="px-6 py-4">
                      <div className="h-6 w-20 animate-pulse rounded bg-gray-200" />
                    </td>
                    <td className="px-6 py-4">
                      <div className="h-8 w-32 animate-pulse rounded bg-gray-200 ml-auto" />
                    </td>
                  </tr>
                ))
              ) : data?.events?.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-6 py-12 text-center text-gray-500">
                    No events found matching your criteria.
                  </td>
                </tr>
              ) : (
                data?.events?.map((event: any) => {
                  const isDeleted = event.isDeleted;
                  const isCancelled = event.status === 1;

                  return (
                    <tr key={event.id} className={isDeleted ? 'bg-red-50' : ''}>
                      <td className="px-6 py-4">
                        <div className={`font-medium break-words ${isDeleted ? 'text-gray-500 line-through' : 'text-gray-900'}`}>
                          {event.title}
                        </div>
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-500">
                        {event.organizerName}
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-500">
                        <div className="whitespace-nowrap">{formatDate(event.startTime)}</div>
                        <div className="whitespace-nowrap text-xs text-gray-400"> {calculateEndTime(event.startTime, event.durationMinutes)}</div>
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-900 whitespace-nowrap">
                        {event.registrationCount} / {event.capacity}
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex flex-col gap-1">
                          <span className={`inline-flex w-fit rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusBadgeColor(event.status)}`}>
                            {event.status === 0 ? 'Active' : 'Cancelled'}
                          </span>
                          {isDeleted && (
                            <span className="inline-flex w-fit rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">
                              Deleted
                            </span>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex justify-end gap-1">
                          <button
                            onClick={() => navigate(`/events/${event.id}/edit`)}
                            disabled={isDeleted}
                            title={isDeleted ? "Cannot edit deleted event" : "Edit event"}
                            className="rounded-md p-2 text-gray-500 hover:bg-gray-100 hover:text-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <PencilIcon className="h-5 w-5" />
                          </button>
                          <button
                            onClick={() => setRegistrationsModalEvent({ id: event.id, title: event.title })}
                            title="View registrations"
                            className="rounded-md p-2 text-gray-500 hover:bg-gray-100 hover:text-green-600"
                          >
                            <EyeIcon className="h-5 w-5" />
                          </button>
                          <button
                            onClick={() => setCancelModalEvent(event)}
                            disabled={isDeleted || isCancelled}
                            title={isDeleted ? "Cannot cancel deleted event" : isCancelled ? "Already cancelled" : "Cancel event"}
                            className="rounded-md p-2 text-gray-500 hover:bg-gray-100 hover:text-amber-600 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <XCircleIcon className="h-5 w-5" />
                          </button>
                          <button
                            onClick={() => setDeleteModalEvent(event)}
                            disabled={isDeleted}
                            title={isDeleted ? "Already deleted" : "Delete event"}
                            className="rounded-md p-2 text-gray-500 hover:bg-gray-100 hover:text-red-600 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <TrashIcon className="h-5 w-5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between border-t border-gray-200 bg-white px-6 py-3">
            <div className="text-sm text-gray-700">
              Showing {((data.page - 1) * data.pageSize) + 1} to {Math.min(data.page * data.pageSize, data.totalCount)} of {data.totalCount} events
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => setPage(page - 1)}
                disabled={!data.hasPreviousPage}
                className="rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="flex items-center px-3 text-sm text-gray-700">
                Page {data.page} of {data.totalPages}
              </span>
              <button
                onClick={() => setPage(page + 1)}
                disabled={!data.hasNextPage}
                className="rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      <DeleteConfirmModal
        event={deleteModalEvent}
        isOpen={deleteModalEvent !== null}
        onClose={() => setDeleteModalEvent(null)}
        onConfirm={handleDelete}
        isLoading={deleteMutation.isPending}
      />

      <CancelConfirmModal
        event={cancelModalEvent}
        isOpen={cancelModalEvent !== null}
        onClose={() => setCancelModalEvent(null)}
        onConfirm={handleCancel}
        isLoading={cancelMutation.isPending}
      />

      <RegistrationsModal
        eventId={registrationsModalEvent?.id || null}
        eventTitle={registrationsModalEvent?.title || ''}
        isOpen={registrationsModalEvent !== null}
        onClose={() => setRegistrationsModalEvent(null)}
      />
    </div>
  );
}
