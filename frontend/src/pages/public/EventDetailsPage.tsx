import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useEvent, useCancelEvent } from '@/hooks/useEvents';
import { useAuth } from '@/hooks/useAuth';
import { useQueryClient } from '@tanstack/react-query';
import { EventStatus, UserRole } from '@/types/enums';
import {
  checkUserRegistration,
  registerForEvent,
  cancelRegistration,
} from '@/services/registrationService';
import { toast } from '@/utils/toast';
import { getImageUrl } from '@/utils/imageUrl';
import { CancelEventModal } from '@/components/modals/CancelEventModal';
import { RegistrationConfirmModal } from '@/components/modals/RegistrationConfirmModal';
import { SkillBadge } from '@/components/skills/SkillBadge';
import { EventDetailsSkeleton } from '@/components/skeletons/EventSkeletons';

export default function EventDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const queryClient = useQueryClient();
  const eventId = Number(id);

  const { data: event, isLoading, isError, error, refetch } = useEvent(eventId);
  const { mutate: cancelEventMutation, isPending: isCancellingEvent } = useCancelEvent();

  const [isRegistered, setIsRegistered] = useState(false);
  const [isRegistering, setIsRegistering] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);
  const [showCancelEventModal, setShowCancelEventModal] = useState(false);
  const [showRegistrationModal, setShowRegistrationModal] = useState(false);

  // Check registration status when event loads
  useEffect(() => {
    if (event && isAuthenticated) {
      checkUserRegistration(event.id).then((result) => {
        setIsRegistered(result.isRegistered);
      });
    }
  }, [event, isAuthenticated]);

  // Calculate if user is event owner or admin
  const isOwner = user && event && user.id === event.organizerId;
  const isAdmin = user?.role === UserRole.Admin;
  const isOrganizer = user?.role === UserRole.Organizer;
  const canEdit = isOwner || isAdmin;

  // Calculate registration eligibility
  const isEventCancelled = event?.status === EventStatus.Cancelled;
  const isEventInPast = event && new Date(event.startTime) < new Date();
  const isRegistrationClosed =
    event?.registrationDeadline && new Date(event.registrationDeadline) < new Date();
  const isFull = event && event.registrationCount >= event.capacity;

  // Check if user has all required skills for this event
  const userSkillIds = user?.skills?.map((s) => s.id) || [];
  const requiredSkillIds = event?.requiredSkills?.map((s) => s.id) || [];
  const hasRequiredSkills =
    requiredSkillIds.length === 0 ||
    requiredSkillIds.every((skillId) => userSkillIds.includes(skillId));
  const missingSkills = event?.requiredSkills?.filter((s) => !userSkillIds.includes(s.id)) || [];

  // Allow registration for:
  // - Volunteers: standard rules apply
  // - Organizers/Admins: can register for events they don't own
  const isVolunteer = user?.role === UserRole.Volunteer;
  const canOrganizerOrAdminRegister = (isOrganizer || isAdmin) && !isOwner;

  const canRegister =
    isAuthenticated &&
    (isVolunteer || canOrganizerOrAdminRegister) &&
    !isRegistered &&
    !isEventCancelled &&
    !isEventInPast &&
    !isRegistrationClosed &&
    !isFull &&
    hasRequiredSkills;

  // Handle registration confirmation
  const handleRegister = async () => {
    if (!event) return;

    setIsRegistering(true);
    try {
      await registerForEvent({ eventId: event.id });
      setIsRegistered(true);
      setShowRegistrationModal(false);

      // Refetch event to update registration count
      await refetch();

      // Invalidate registrations cache so My Events page shows the new registration
      queryClient.invalidateQueries({ queryKey: ['registrations', 'me'] });

      toast.success(`Successfully registered for ${event.title}!`);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Failed to register for event. Please try again.';
      toast.error(errorMessage);
    } finally {
      setIsRegistering(false);
    }
  };

  // Handle cancel registration
  const handleCancelRegistration = async () => {
    if (!event) return;

    setIsCancelling(true);
    try {
      await cancelRegistration(event.id);
      setIsRegistered(false);

      // Refetch event to update registration count
      await refetch();

      // Invalidate registrations cache so My Events page updates
      queryClient.invalidateQueries({ queryKey: ['registrations', 'me'] });

      toast.success('Registration cancelled successfully');
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Failed to cancel registration. Please try again.';
      toast.error(errorMessage);
    } finally {
      setIsCancelling(false);
    }
  };

  // Handle cancel event (set status to Cancelled)
  const handleCancelEvent = () => {
    if (!event) return;

    cancelEventMutation(event.id, {
      onSuccess: () => {
        setShowCancelEventModal(false);
        toast.success('Event cancelled successfully');
      },
      onError: () => {
        toast.error('Failed to cancel event. Please try again.');
      },
    });
  };

  // Format date and time
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
    if (hours > 0 && mins > 0)
      return `${hours} hour${hours > 1 ? 's' : ''} ${mins} minute${mins > 1 ? 's' : ''}`;
    if (hours > 0) return `${hours} hour${hours > 1 ? 's' : ''}`;
    return `${mins} minute${mins > 1 ? 's' : ''}`;
  };

  // Create Google Maps link
  const getMapLink = (location: string) => {
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(location)}`;
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="mx-auto max-w-4xl px-4">
          <EventDetailsSkeleton />
        </div>
      </div>
    );
  }

  // Error state
  if (isError || !event) {
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
            <h2 className="mt-4 text-xl font-semibold text-red-900">Event not found</h2>
            <p className="mt-2 text-sm text-red-700">
              {error instanceof Error
                ? error.message
                : 'The event you are looking for does not exist.'}
            </p>
            <Link
              to="/events"
              className="mt-6 inline-block rounded-lg bg-red-600 px-6 py-3 min-h-[44px] min-w-[44px] text-center text-sm font-medium text-white transition-all hover:bg-red-700 hover:shadow-md active:scale-95"
            >
              Back to Events
            </Link>
          </div>
        </div>
      </div>
    );
  }

  const availableSpots = event.capacity - event.registrationCount;
  const isNearlyFull = !isFull && event.registrationCount / event.capacity > 0.8;

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="mx-auto max-w-4xl px-4">
        {/* Back button */}
        <Link
          to="/events"
          className="mb-6 inline-flex items-center gap-2 text-sm font-medium text-gray-600 transition-colors hover:text-gray-900"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M15 19l-7-7 7-7"
            />
          </svg>
          Back to Events
        </Link>

        {/* Event image */}
        <div
          className="mb-6 overflow-hidden rounded-lg bg-gray-100"
          style={{ height: '15vh', minHeight: '150px' }}
        >
          {event.imageUrl ? (
            <img
              src={getImageUrl(event.imageUrl)}
              alt={event.title}
              className="h-full w-full object-cover"
            />
          ) : (
            <div className="flex h-full items-center justify-center bg-gradient-to-br from-blue-500 to-blue-600">
              <svg
                className="h-16 w-16 text-white opacity-50"
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
            </div>
          )}
        </div>

        {/* Main content */}
        <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          {/* Cancelled banner */}
          {isEventCancelled && (
            <div className="mb-6 rounded-lg border border-amber-300 bg-amber-50 px-4 py-3 flex items-start gap-3">
              <svg
                className="h-6 w-6 text-amber-600 flex-shrink-0 mt-0.5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
              <div>
                <p className="font-semibold text-amber-900">This event has been cancelled</p>
                <p className="mt-1 text-sm text-amber-800">
                  This event is no longer accepting registrations and will not take place as
                  scheduled.
                </p>
              </div>
            </div>
          )}

          {/* Nearly full banner */}
          {!isEventCancelled && !isFull && isNearlyFull && (
            <div className="mb-6 rounded-lg border border-orange-300 bg-orange-50 px-4 py-3 flex items-start gap-3">
              <svg
                className="h-6 w-6 text-orange-600 flex-shrink-0 mt-0.5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <div>
                <p className="font-semibold text-orange-900">
                  Almost Full - Only {availableSpots} spot{availableSpots !== 1 ? 's' : ''}{' '}
                  remaining!
                </p>
                <p className="mt-1 text-sm text-orange-800">
                  This event is filling up fast. Register soon to secure your spot.
                </p>
              </div>
            </div>
          )}

          {/* Status badge */}
          {isEventCancelled && (
            <div className="mb-4">
              <span className="inline-flex rounded-full bg-red-100 px-3 py-1 text-sm font-semibold text-red-800">
                Event Cancelled
              </span>
            </div>
          )}

          {/* Title */}
          <h1 className="text-3xl font-bold text-gray-900">{event.title}</h1>

          {/* Organizer */}
          <div className="mt-2 flex items-center gap-2 text-gray-600">
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
              />
            </svg>
            <span>Organized by {event.organizerName}</span>
          </div>

          {/* Date, time, and duration */}
          <div className="mt-6 grid gap-4 sm:grid-cols-2">
            <div className="flex items-start gap-3">
              <svg
                className="mt-0.5 h-5 w-5 text-gray-400"
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
              <div>
                <p className="text-sm font-medium text-gray-500">Date & Time</p>
                <p className="mt-1 text-gray-900">
                  {formatDate(event.startTime)} at {formatTime(event.startTime)}
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <svg
                className="mt-0.5 h-5 w-5 text-gray-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <div>
                <p className="text-sm font-medium text-gray-500">Duration</p>
                <p className="mt-1 text-gray-900">{formatDuration(event.durationMinutes)}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <svg
                className="mt-0.5 h-5 w-5 text-gray-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                />
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                />
              </svg>
              <div>
                <p className="text-sm font-medium text-gray-500">Location</p>
                <a
                  href={getMapLink(event.location)}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="mt-1 text-blue-600 hover:text-blue-700 hover:underline"
                >
                  {event.location}
                </a>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <svg
                className="mt-0.5 h-5 w-5 text-gray-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
                />
              </svg>
              <div>
                <p className="text-sm font-medium text-gray-500">Capacity</p>
                <p className="mt-1 text-gray-900">
                  {event.registrationCount} / {event.capacity} registered
                  {isFull && <span className="ml-2 text-orange-600 font-semibold">(Full)</span>}
                  {!isFull && (
                    <span className="ml-2 text-gray-600">
                      ({availableSpots} spot{availableSpots !== 1 ? 's' : ''} left)
                    </span>
                  )}
                </p>
              </div>
            </div>
          </div>

          {/* Registration deadline */}
          {event.registrationDeadline && (
            <div className="mt-4 flex items-start gap-3">
              <svg
                className="mt-0.5 h-5 w-5 text-gray-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <div>
                <p className="text-sm font-medium text-gray-500">Registration Deadline</p>
                <p
                  className={`mt-1 ${isRegistrationClosed ? 'text-red-600 font-semibold' : 'text-gray-900'}`}
                >
                  {formatDate(event.registrationDeadline)} at{' '}
                  {formatTime(event.registrationDeadline)}
                  {isRegistrationClosed && ' - Registration Closed'}
                </p>
              </div>
            </div>
          )}

          {/* Description */}
          <div className="mt-8">
            <h2 className="text-lg font-semibold text-gray-900">About this event</h2>
            <p className="mt-3 whitespace-pre-wrap text-gray-700 leading-relaxed">
              {event.description}
            </p>
          </div>

          {/* Required skills */}
          {event.requiredSkills.length > 0 && (
            <div className="mt-8">
              <h2 className="text-lg font-semibold text-gray-900 mb-1">Required Skills</h2>
              <p className="text-sm text-gray-600 mb-3">
                Volunteers should have the following skills:
              </p>
              <div className="flex flex-wrap gap-2">
                {event.requiredSkills.map((skill) => (
                  <SkillBadge key={skill.id} skill={skill} size="md" showTooltip={true} />
                ))}
              </div>
            </div>
          )}

          {/* Action buttons */}
          <div className="mt-8 flex flex-wrap gap-3">
            {/* Register/Cancel registration buttons for eligible users */}
            {/* Volunteers can always register, Organizers/Admins can register for events they don't own */}
            {isAuthenticated &&
              (isVolunteer || canOrganizerOrAdminRegister) &&
              !isEventCancelled && (
                <>
                  {isRegistered ? (
                    <>
                      {/* Already registered indicator */}
                      <div className="flex items-center gap-2 rounded-lg bg-green-50 px-4 py-3 text-green-800">
                        <svg
                          className="h-5 w-5"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M5 13l4 4L19 7"
                          />
                        </svg>
                        <span className="font-medium">You are registered for this event</span>
                      </div>
                      <button
                        onClick={handleCancelRegistration}
                        disabled={isCancelling}
                        className="rounded-lg border border-red-600 bg-white px-6 py-3 font-medium text-red-600 transition-colors hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        {isCancelling ? 'Cancelling...' : 'Cancel Registration'}
                      </button>
                    </>
                  ) : (
                    <>
                      <button
                        onClick={() => setShowRegistrationModal(true)}
                        disabled={!canRegister}
                        className="rounded-lg bg-blue-600 px-6 py-3 font-medium text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-gray-400"
                      >
                        Register for Event
                      </button>
                      {!canRegister && !isEventCancelled && !isRegistered && (
                        <p className="flex items-center text-sm text-gray-600">
                          {isOwner && 'You cannot register for your own event'}
                          {!isOwner && isEventInPast && 'This event has already occurred'}
                          {!isOwner &&
                            isRegistrationClosed &&
                            !isEventInPast &&
                            'Registration is closed'}
                          {!isOwner &&
                            isFull &&
                            !isRegistrationClosed &&
                            !isEventInPast &&
                            'This event is full'}
                          {!isOwner &&
                            !hasRequiredSkills &&
                            !isFull &&
                            !isRegistrationClosed &&
                            !isEventInPast && (
                              <span className="text-amber-600">
                                Missing required skills:{' '}
                                {missingSkills.map((s) => s.name).join(', ')}
                              </span>
                            )}
                        </p>
                      )}
                    </>
                  )}
                </>
              )}

            {/* Message for event owners */}
            {isAuthenticated && isOwner && !isEventCancelled && (
              <div className="flex items-center gap-2 rounded-lg bg-gray-50 px-4 py-3 text-gray-600">
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
                <span className="text-sm">You are the organizer of this event</span>
              </div>
            )}

            {/* Prompt to login for guests */}
            {!isAuthenticated && (
              <div className="w-full rounded-lg border border-blue-200 bg-blue-50 p-4">
                <p className="text-sm text-blue-900">
                  <Link to="/login" className="font-medium underline hover:text-blue-700">
                    Log in
                  </Link>{' '}
                  or{' '}
                  <Link to="/register" className="font-medium underline hover:text-blue-700">
                    create an account
                  </Link>{' '}
                  to register for this event.
                </p>
              </div>
            )}

            {/* Edit button for owner/admin */}
            {canEdit && (
              <button
                onClick={() => navigate(`/events/${event.id}/edit`)}
                className="rounded-lg border border-gray-300 bg-white px-6 py-3 font-medium text-gray-700 transition-colors hover:bg-gray-50"
              >
                Edit Event
              </button>
            )}

            {/* Cancel event button for owner/admin */}
            {canEdit && !isEventCancelled && (
              <button
                onClick={() => setShowCancelEventModal(true)}
                className="rounded-lg border border-amber-600 bg-white px-6 py-3 font-medium text-amber-700 transition-colors hover:bg-amber-50"
              >
                Cancel Event
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Cancel event confirmation modal */}
      <CancelEventModal
        isOpen={showCancelEventModal}
        onClose={() => setShowCancelEventModal(false)}
        onConfirm={handleCancelEvent}
        eventTitle={event?.title ?? ''}
        isLoading={isCancellingEvent}
      />

      {/* Registration confirmation modal */}
      {event && (
        <RegistrationConfirmModal
          event={event}
          isOpen={showRegistrationModal}
          onClose={() => setShowRegistrationModal(false)}
          onConfirm={handleRegister}
          isLoading={isRegistering}
        />
      )}
    </div>
  );
}
