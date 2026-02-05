import { Link } from 'react-router-dom';
import type { EventResponse } from '@/services/eventService';
import { EventStatus } from '@/types/enums';
import { SkillBadgeList } from '@/components/skills/SkillBadge';

interface EventCardProps {
  event: EventResponse;
}

export function EventCard({ event }: EventCardProps) {
  // Format date and time
  const eventDate = new Date(event.startTime);
  const formattedDate = eventDate.toLocaleDateString('en-US', {
    weekday: 'short',
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
  const formattedTime = eventDate.toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
  });

  // Calculate duration display
  const hours = Math.floor(event.durationMinutes / 60);
  const minutes = event.durationMinutes % 60;
  const durationDisplay =
    hours > 0 ? `${hours}h${minutes > 0 ? ` ${minutes}m` : ''}` : `${minutes}m`;

  // Calculate capacity status
  const availableSpots = event.capacity - event.registrationCount;
  const capacityPercentage = (event.registrationCount / event.capacity) * 100;
  const isFull = availableSpots <= 0;
  const isAlmostFull = capacityPercentage > 80 && !isFull;

  // Check if event is cancelled
  const isCancelled = event.status === EventStatus.Cancelled;

  return (
    <Link
      to={`/events/${event.id}`}
      className="group block rounded-lg border border-gray-200 bg-white shadow-sm transition-all duration-200 hover:shadow-md hover:border-gray-300 hover:-translate-y-1 min-h-[400px]"
    >
      <div className="flex flex-col h-full relative">
        {/* Gray overlay for cancelled events */}
        {isCancelled && (
          <div className="absolute inset-0 bg-gray-900 bg-opacity-40 rounded-lg z-10 flex items-center justify-center">
            <div className="bg-white bg-opacity-95 px-6 py-3 rounded-lg shadow-lg">
              <span className="text-xl font-bold text-gray-800">CANCELLED</span>
            </div>
          </div>
        )}

        {/* Event image or placeholder */}
        <div className="relative h-48 overflow-hidden rounded-t-lg bg-gray-100">
          {event.imageUrl ? (
            <img
              src={event.imageUrl}
              alt={event.title}
              className="h-full w-full object-cover transition-transform group-hover:scale-105"
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

          {/* Cancelled badge */}
          {isCancelled && (
            <div className="absolute top-2 right-2">
              <span className="inline-flex rounded-full bg-red-100 px-3 py-1 text-xs font-semibold text-red-800">
                Cancelled
              </span>
            </div>
          )}

          {/* Full badge */}
          {!isCancelled && isFull && (
            <div className="absolute top-2 right-2">
              <span className="inline-flex rounded-full bg-orange-100 px-3 py-1 text-xs font-semibold text-orange-800">
                Full
              </span>
            </div>
          )}

          {/* Nearly full badge */}
          {!isCancelled && !isFull && isAlmostFull && (
            <div className="absolute top-2 right-2">
              <span className="inline-flex rounded-full bg-yellow-100 px-3 py-1 text-xs font-semibold text-yellow-800">
                Almost Full
              </span>
            </div>
          )}
        </div>

        {/* Card content */}
        <div className="flex flex-1 flex-col p-4">
          {/* Title */}
          <h3 className="text-lg font-semibold text-gray-900 group-hover:text-blue-600 transition-colors line-clamp-2">
            {event.title}
          </h3>

          {/* Date and time */}
          <div className="mt-2 flex items-center gap-2 text-sm text-gray-600">
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
              />
            </svg>
            <span>
              {formattedDate} at {formattedTime}
            </span>
          </div>

          {/* Location */}
          <div className="mt-1 flex items-center gap-2 text-sm text-gray-600">
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
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
            <span className="line-clamp-1">{event.location}</span>
          </div>

          {/* Duration */}
          <div className="mt-1 flex items-center gap-2 text-sm text-gray-600">
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <span>{durationDisplay}</span>
          </div>

          {/* Capacity */}
          <div className="mt-3">
            <div className="flex items-center justify-between text-sm">
              <span className="text-gray-600">Capacity</span>
              <span
                className={`font-medium ${
                  isFull ? 'text-orange-600' : isAlmostFull ? 'text-yellow-600' : 'text-green-600'
                }`}
              >
                {event.registrationCount} / {event.capacity}
              </span>
            </div>
            <div className="mt-1 h-2 w-full rounded-full bg-gray-200">
              <div
                className={`h-full rounded-full transition-all ${
                  isFull ? 'bg-orange-500' : isAlmostFull ? 'bg-yellow-500' : 'bg-green-500'
                }`}
                style={{ width: `${Math.min(capacityPercentage, 100)}%` }}
              />
            </div>
          </div>

          {/* Required skills */}
          {event.requiredSkills.length > 0 && (
            <div className="mt-4 flex-1">
              <h4 className="text-xs font-semibold text-gray-700 mb-2">Required Skills</h4>
              <SkillBadgeList 
                skills={event.requiredSkills} 
                maxVisible={3}
                showTooltips={true}
              />
            </div>
          )}

          {/* Organizer */}
          <div className="mt-4 pt-4 border-t border-gray-100">
            <div className="flex items-center gap-2 text-sm text-gray-600">
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                />
              </svg>
              <span>Organized by {event.organizerName}</span>
            </div>
          </div>
        </div>
      </div>
    </Link>
  );
}
