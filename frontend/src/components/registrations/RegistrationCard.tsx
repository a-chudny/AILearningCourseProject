import { Link } from 'react-router-dom';
import { RegistrationResponse } from '@/services/registrationService';
import { EventStatus } from '@/types/enums';

interface RegistrationCardProps {
  registration: RegistrationResponse;
  showCancelButton?: boolean;
  onCancelClick?: () => void;
}

/**
 * Card displaying a user's registration with event summary
 */
export function RegistrationCard({
  registration,
  showCancelButton = false,
  onCancelClick,
}: RegistrationCardProps) {
  const { event, status, registeredAt } = registration;

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatDuration = (minutes: number) => {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    if (hours > 0 && mins > 0) return `${hours}h ${mins}m`;
    if (hours > 0) return `${hours}h`;
    return `${mins}m`;
  };

  const getStatusBadgeClasses = () => {
    if (status === 'Confirmed') {
      return 'bg-green-100 text-green-800';
    }
    return 'bg-gray-100 text-gray-800';
  };

  const isEventCancelled = event.status === EventStatus.Cancelled;

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm transition-shadow hover:shadow-md">
      <div className="flex gap-4">
        {/* Event image or placeholder */}
        <Link to={`/events/${event.id}`} className="flex-shrink-0">
          {event.imageUrl ? (
            <img
              src={event.imageUrl}
              alt={event.title}
              className="h-24 w-24 rounded-lg object-cover"
            />
          ) : (
            <div className="flex h-24 w-24 items-center justify-center rounded-lg bg-gradient-to-br from-blue-500 to-blue-600">
              <svg className="h-10 w-10 text-white opacity-50" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            </div>
          )}
        </Link>

        {/* Event details */}
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-3">
            <div className="flex-1 min-w-0">
              <Link
                to={`/events/${event.id}`}
                className="text-lg font-semibold text-gray-900 hover:text-blue-600 transition-colors line-clamp-2"
              >
                {event.title}
              </Link>

              {/* Status badges */}
              <div className="mt-2 flex flex-wrap gap-2">
                <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusBadgeClasses()}`}>
                  {status}
                </span>
                {isEventCancelled && (
                  <span className="inline-flex items-center rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">
                    Event Cancelled
                  </span>
                )}
              </div>
            </div>

            {/* Cancel button */}
            {showCancelButton && !isEventCancelled && (
              <button
                onClick={onCancelClick}
                className="flex-shrink-0 rounded-lg border border-red-600 bg-white px-4 py-2 text-sm font-medium text-red-600 transition-colors hover:bg-red-50"
              >
                Cancel
              </button>
            )}
          </div>

          {/* Event metadata */}
          <div className="mt-3 space-y-1.5">
            {/* Date and time */}
            <div className="flex items-center gap-2 text-sm text-gray-600">
              <svg className="h-4 w-4 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
              <span>
                {formatDate(event.startTime)} at {formatTime(event.startTime)} • {formatDuration(event.durationMinutes)}
              </span>
            </div>

            {/* Location */}
            <div className="flex items-center gap-2 text-sm text-gray-600">
              <svg className="h-4 w-4 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <span className="truncate">{event.location}</span>
            </div>

            {/* Registration date */}
            <div className="flex items-center gap-2 text-sm text-gray-500">
              <svg className="h-4 w-4 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <span>Registered on {formatDate(registeredAt)}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
