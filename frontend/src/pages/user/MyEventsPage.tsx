import { useState, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useMyRegistrations, useCancelRegistration } from '@/hooks/useRegistrations';
import { RegistrationCard } from '@/components/registrations/RegistrationCard';
import { CancelRegistrationModal } from '@/components/modals/CancelRegistrationModal';
import { RegistrationResponse, EventSummary } from '@/services/registrationService';

export default function MyEventsPage() {
  const { data: registrations, isLoading, isError, error } = useMyRegistrations();
  const { mutate: cancelRegistration, isPending: isCancelling } = useCancelRegistration();

  const [cancelModalOpen, setCancelModalOpen] = useState(false);
  const [selectedEvent, setSelectedEvent] = useState<EventSummary | null>(null);
  const [cancellingEventId, setCancellingEventId] = useState<number | null>(null);

  // Group registrations by status and time
  const groupedRegistrations = useMemo(() => {
    if (!registrations) {
      return {
        upcomingConfirmed: [],
        pastConfirmed: [],
        cancelled: [],
      };
    }

    const now = new Date();
    const upcomingConfirmed: RegistrationResponse[] = [];
    const pastConfirmed: RegistrationResponse[] = [];
    const cancelled: RegistrationResponse[] = [];

    registrations.forEach((reg) => {
      const eventDate = new Date(reg.event.startTime);

      if (reg.status === 'Cancelled') {
        cancelled.push(reg);
      } else if (eventDate >= now) {
        upcomingConfirmed.push(reg);
      } else {
        pastConfirmed.push(reg);
      }
    });

    // Sort upcoming: nearest first
    upcomingConfirmed.sort(
      (a, b) => new Date(a.event.startTime).getTime() - new Date(b.event.startTime).getTime()
    );

    // Sort past: most recent first
    pastConfirmed.sort(
      (a, b) => new Date(b.event.startTime).getTime() - new Date(a.event.startTime).getTime()
    );

    // Sort cancelled: most recent cancellation first
    cancelled.sort(
      (a, b) => new Date(b.registeredAt).getTime() - new Date(a.registeredAt).getTime()
    );

    return { upcomingConfirmed, pastConfirmed, cancelled };
  }, [registrations]);

  // Handle cancel click
  const handleCancelClick = (event: EventSummary) => {
    setSelectedEvent(event);
    setCancelModalOpen(true);
  };

  // Confirm cancellation
  const handleConfirmCancel = () => {
    if (selectedEvent) {
      setCancellingEventId(selectedEvent.id);
      cancelRegistration(selectedEvent.id, {
        onSettled: () => {
          setCancelModalOpen(false);
          setSelectedEvent(null);
          setCancellingEventId(null);
        },
      });
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50">
        <div className="flex flex-col items-center gap-4">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
          <p className="text-gray-600">Loading your events...</p>
        </div>
      </div>
    );
  }

  // Error state
  if (isError) {
    return (
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="mx-auto max-w-4xl px-4">
          <div className="rounded-lg border border-red-200 bg-red-50 p-8 text-center">
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
            <h2 className="mt-4 text-xl font-semibold text-red-900">Failed to load registrations</h2>
            <p className="mt-2 text-sm text-red-700">
              {error instanceof Error ? error.message : 'An error occurred while loading your events.'}
            </p>
            <button
              onClick={() => window.location.reload()}
              className="mt-6 inline-block rounded-lg bg-red-600 px-6 py-2 text-sm font-medium text-white transition-colors hover:bg-red-700"
            >
              Try Again
            </button>
          </div>
        </div>
      </div>
    );
  }

  // Empty state
  if (!registrations || registrations.length === 0) {
    return (
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="mx-auto max-w-4xl px-4">
          <h1 className="text-3xl font-bold text-gray-900">My Events</h1>
          <div className="mt-8 rounded-lg border border-gray-200 bg-white p-12 text-center">
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
            <h2 className="mt-4 text-xl font-semibold text-gray-900">No events yet</h2>
            <p className="mt-2 text-gray-600">
              You haven't registered for any events. Browse available events to get started!
            </p>
          </div>
        </div>
      </div>
    );
  }

  const { upcomingConfirmed, pastConfirmed, cancelled } = groupedRegistrations;
  const hasAnyEvents = upcomingConfirmed.length > 0 || pastConfirmed.length > 0 || cancelled.length > 0;

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="mx-auto max-w-4xl px-4">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">My Events</h1>
          <p className="mt-2 text-gray-600">Manage your event registrations</p>
        </div>

        {/* Empty state if no events after grouping */}
        {!hasAnyEvents && (
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
            <h2 className="mt-4 text-xl font-semibold text-gray-900">No events yet</h2>
            <p className="mt-2 text-gray-600">
              You haven't registered for any events. Browse available events to get started!
            </p>
          </div>
        )}

        {/* Upcoming Events Section */}
        {upcomingConfirmed.length > 0 && (
          <section className="mb-8">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-2xl font-bold text-gray-900">
                Upcoming Events
                <span className="ml-2 text-base font-normal text-gray-600">
                  ({upcomingConfirmed.length})
                </span>
              </h2>
            </div>
            <div className="space-y-4">
              {upcomingConfirmed.map((registration) => (
                <RegistrationCard
                  key={registration.id}
                  registration={registration}
                  showCancelButton={true}
                  onCancelClick={() => handleCancelClick(registration.event)}
                />
              ))}
            </div>
          </section>
        )}

        {/* Past Events Section */}
        {pastConfirmed.length > 0 && (
          <section className="mb-8">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-2xl font-bold text-gray-900">
                Past Events
                <span className="ml-2 text-base font-normal text-gray-600">
                  ({pastConfirmed.length})
                </span>
              </h2>
            </div>
            <div className="space-y-4">
              {pastConfirmed.map((registration) => (
                <RegistrationCard
                  key={registration.id}
                  registration={registration}
                  showCancelButton={false}
                />
              ))}
            </div>
          </section>
        )}

        {/* Cancelled Registrations Section */}
        {cancelled.length > 0 && (
          <section className="mb-8">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-2xl font-bold text-gray-900">
                Cancelled Registrations
                <span className="ml-2 text-base font-normal text-gray-600">
                  ({cancelled.length})
                </span>
              </h2>
            </div>
            <div className="space-y-4">
              {cancelled.map((registration) => (
                <RegistrationCard
                  key={registration.id}
                  registration={registration}
                  showCancelButton={false}
                />
              ))}
            </div>
          </section>
        )}

        {/* Browse events link */}
        {hasAnyEvents && (
          <div className="mt-8 text-center">
            <Link
              to="/events"
              className="inline-flex items-center gap-2 text-blue-600 hover:text-blue-700 font-medium"
            >
              <span>Browse more events</span>
              <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
              </svg>
            </Link>
          </div>
        )}
      </div>

      {/* Cancel confirmation modal */}
      {selectedEvent && (
        <CancelRegistrationModal
          event={selectedEvent}
          isOpen={cancelModalOpen}
          onClose={() => {
            setCancelModalOpen(false);
            setSelectedEvent(null);
          }}
          onConfirm={handleConfirmCancel}
          isLoading={isCancelling && cancellingEventId === selectedEvent.id}
        />
      )}
    </div>
  );
}
