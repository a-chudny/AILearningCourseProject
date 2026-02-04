import { EventSummary } from '@/services/registrationService';

interface CancelRegistrationModalProps {
  event: EventSummary;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isLoading: boolean;
}

/**
 * Confirmation modal for cancelling event registration
 * Shows event details and asks for confirmation before cancelling
 */
export function CancelRegistrationModal({
  event,
  isOpen,
  onClose,
  onConfirm,
  isLoading,
}: CancelRegistrationModalProps) {
  if (!isOpen) return null;

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
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
    if (hours > 0 && mins > 0) return `${hours} hour${hours > 1 ? 's' : ''} ${mins} minute${mins > 1 ? 's' : ''}`;
    if (hours > 0) return `${hours} hour${hours > 1 ? 's' : ''}`;
    return `${mins} minute${mins > 1 ? 's' : ''}`;
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
      <div className="w-full max-w-lg overflow-hidden rounded-lg bg-white shadow-xl">
        {/* Header */}
        <div className="border-b border-gray-200 bg-white px-6 py-4">
          <div className="flex items-start justify-between">
            <div>
              <h2 className="text-xl font-bold text-gray-900">Cancel Registration</h2>
              <p className="mt-1 text-sm text-gray-600">Are you sure you want to cancel your registration?</p>
            </div>
            <button
              onClick={onClose}
              disabled={isLoading}
              className="text-gray-400 transition-colors hover:text-gray-600 disabled:cursor-not-allowed disabled:opacity-50"
              aria-label="Close modal"
            >
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        {/* Body */}
        <div className="px-6 py-4">
          {/* Event title */}
          <div className="mb-4">
            <h3 className="text-lg font-bold text-gray-900">{event.title}</h3>
          </div>

          {/* Event details */}
          <div className="space-y-3">
            {/* Date & Time */}
            <div className="flex items-start gap-3">
              <svg className="mt-0.5 h-5 w-5 flex-shrink-0 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
              <div>
                <p className="text-sm text-gray-900">
                  {formatDate(event.startTime)} at {formatTime(event.startTime)}
                </p>
              </div>
            </div>

            {/* Duration */}
            <div className="flex items-start gap-3">
              <svg className="mt-0.5 h-5 w-5 flex-shrink-0 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <div>
                <p className="text-sm text-gray-900">{formatDuration(event.durationMinutes)}</p>
              </div>
            </div>

            {/* Location */}
            <div className="flex items-start gap-3">
              <svg className="mt-0.5 h-5 w-5 flex-shrink-0 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <div>
                <p className="text-sm text-gray-900">{event.location}</p>
              </div>
            </div>
          </div>

          {/* Warning message */}
          <div className="mt-4 rounded-lg border border-amber-300 bg-amber-50 p-3">
            <div className="flex gap-3">
              <svg className="h-5 w-5 flex-shrink-0 text-amber-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
              <p className="text-sm text-amber-800">
                This action cannot be undone. You'll need to register again if you change your mind.
              </p>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="border-t border-gray-200 bg-gray-50 px-6 py-4">
          <div className="flex justify-end gap-3">
            <button
              onClick={onClose}
              disabled={isLoading}
              className="rounded-lg border border-gray-300 bg-white px-6 py-2 font-medium text-gray-700 transition-colors hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Keep Registration
            </button>
            <button
              onClick={onConfirm}
              disabled={isLoading}
              className="flex items-center gap-2 rounded-lg bg-red-600 px-6 py-2 font-medium text-white transition-colors hover:bg-red-700 disabled:cursor-not-allowed disabled:bg-red-400"
            >
              {isLoading && (
                <svg className="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path
                    className="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                  />
                </svg>
              )}
              {isLoading ? 'Cancelling...' : 'Cancel Registration'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
