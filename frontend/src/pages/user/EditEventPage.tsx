import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { EventForm, type EventFormData } from '@/components/events/forms/EventForm';
import { useEvent, useUpdateEvent } from '@/hooks/useEvents';
import { useAuth } from '@/hooks/useAuth';
import { UserRole } from '@/types';
import type { Skill } from '@/types';
import type { UpdateEventRequest, EventResponse } from '@/services/eventService';
import { uploadEventImage, deleteEventImage } from '@/services/imageService';
import { getImageUrl } from '@/utils/imageUrl';

/**
 * Modal component for unsaved changes confirmation
 */
interface UnsavedChangesModalProps {
  isOpen: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

function UnsavedChangesModal({ isOpen, onConfirm, onCancel }: UnsavedChangesModalProps) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
        <h3 className="text-lg font-semibold mb-2">Unsaved Changes</h3>
        <p className="text-gray-600 mb-6">
          You have unsaved changes. Are you sure you want to leave this page? Your changes will be
          lost.
        </p>
        <div className="flex justify-end gap-3">
          <button
            onClick={onCancel}
            className="px-4 py-2 text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200"
          >
            Stay on Page
          </button>
          <button
            onClick={onConfirm}
            className="px-4 py-2 text-white bg-red-600 rounded-md hover:bg-red-700"
          >
            Leave Page
          </button>
        </div>
      </div>
    </div>
  );
}

/**
 * Skeleton loading component for the form
 */
function FormSkeleton() {
  return (
    <div className="bg-white rounded-lg shadow-md p-6 animate-pulse">
      <div className="h-8 bg-gray-200 rounded w-1/3 mb-4"></div>
      <div className="space-y-4">
        <div className="h-10 bg-gray-200 rounded"></div>
        <div className="h-32 bg-gray-200 rounded"></div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="h-10 bg-gray-200 rounded"></div>
          <div className="h-10 bg-gray-200 rounded"></div>
        </div>
        <div className="h-10 bg-gray-200 rounded"></div>
        <div className="h-10 bg-gray-200 rounded"></div>
        <div className="flex justify-end gap-3 mt-6">
          <div className="h-10 bg-gray-200 rounded w-24"></div>
          <div className="h-10 bg-gray-200 rounded w-32"></div>
        </div>
      </div>
    </div>
  );
}

/**
 * Edit Event Page Component
 * Allows event owners and admins to edit event details
 */
export default function EditEventPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const eventId = parseInt(id || '0', 10);

  const { data: event, isLoading, isError } = useEvent(eventId);
  const updateEvent = useUpdateEvent();

  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [showUnsavedModal, setShowUnsavedModal] = useState(false);
  const [pendingNavigation, setPendingNavigation] = useState<(() => void) | null>(null);

  // Check ownership and redirect if unauthorized
  useEffect(() => {
    if (!isLoading && event && user) {
      const isOwner = event.organizerId === user.id;
      const isAdmin = user.role === UserRole.Admin;

      if (!isOwner && !isAdmin) {
        // Redirect to 404 if not owner or admin
        navigate('/404', { replace: true });
      }
    }
  }, [event, user, isLoading, navigate]);

  // Redirect to 404 if event not found
  useEffect(() => {
    if (isError) {
      navigate('/404', { replace: true });
    }
  }, [isError, navigate]);

  /**
   * Convert event data to form data format
   */
  const eventToFormData = (eventData: EventResponse): Partial<EventFormData> => {
    const eventDateTime = new Date(eventData.startTime);
    const eventDate = eventDateTime.toISOString().split('T')[0];
    const eventTime = eventDateTime.toTimeString().slice(0, 5); // HH:MM format

    let deadlineDate = '';
    let deadlineTime = '';
    if (eventData.registrationDeadline) {
      const deadlineDateTime = new Date(eventData.registrationDeadline);
      deadlineDate = deadlineDateTime.toISOString().split('T')[0];
      deadlineTime = deadlineDateTime.toTimeString().slice(0, 5);
    }

    // Check if duration matches a preset
    const matchingPreset = [60, 120, 240, 480].includes(eventData.durationMinutes);

    return {
      title: eventData.title,
      description: eventData.description,
      location: eventData.location,
      date: eventDate,
      time: eventTime,
      durationMinutes: matchingPreset ? eventData.durationMinutes : 0,
      customDuration: matchingPreset ? '' : eventData.durationMinutes.toString(),
      capacity: eventData.capacity,
      imagePreview: getImageUrl(eventData.imageUrl) || '',
      registrationDeadlineDate: deadlineDate,
      registrationDeadlineTime: deadlineTime,
      requiredSkills: eventData.requiredSkills,
    };
  };

  /**
   * Handle form submission
   */
  const handleSubmit = async (formData: EventFormData) => {
    if (!event) return;

    try {
      // Combine date and time into ISO 8601 format with UTC timezone marker
      const startTime = `${formData.date}T${formData.time}:00Z`;

      // Combine registration deadline if provided
      let registrationDeadline: string | undefined;
      if (formData.registrationDeadlineDate && formData.registrationDeadlineTime) {
        registrationDeadline = `${formData.registrationDeadlineDate}T${formData.registrationDeadlineTime}:00Z`;
      }

      // Get duration in minutes
      const durationMinutes =
        formData.durationMinutes === 0
          ? parseInt(formData.customDuration, 10)
          : formData.durationMinutes;

      // Extract skill IDs
      const requiredSkillIds = formData.requiredSkills.map((skill: Skill) => skill.id);

      // Prepare update request
      const updateRequest: UpdateEventRequest = {
        title: formData.title,
        description: formData.description,
        location: formData.location,
        startTime,
        durationMinutes,
        capacity: formData.capacity,
        registrationDeadline,
        requiredSkillIds,
        status: event.status, // Keep current status
        // Note: Image upload will be handled separately
      };

      // Submit update
      await updateEvent.mutateAsync({ id: eventId, data: updateRequest });

      // Handle image upload/deletion
      if (formData.imageFile) {
        // New image selected - upload it
        try {
          await uploadEventImage(eventId, formData.imageFile);
        } catch (imageError) {
          console.error('Failed to upload image:', imageError);
          // Don't fail the whole operation if just image upload fails
        }
      } else if (!formData.imagePreview && event.imageUrl) {
        // Image was removed - delete it
        try {
          await deleteEventImage(eventId);
        } catch (imageError) {
          console.error('Failed to delete image:', imageError);
        }
      }

      // Clear unsaved changes flag
      setHasUnsavedChanges(false);

      // Redirect to event details with success message
      navigate(`/events/${eventId}`, {
        state: { successMessage: 'Event updated successfully!' },
      });
    } catch (error) {
      // Re-throw to let EventForm handle error display
      throw error;
    }
  };

  /**
   * Handle cancel with unsaved changes check
   */
  const handleCancel = () => {
    if (hasUnsavedChanges) {
      setPendingNavigation(() => () => navigate(-1));
      setShowUnsavedModal(true);
    } else {
      navigate(-1);
    }
  };

  /**
   * Confirm navigation with unsaved changes
   */
  const handleConfirmNavigation = () => {
    setShowUnsavedModal(false);
    if (pendingNavigation) {
      pendingNavigation();
      setPendingNavigation(null);
    }
  };

  /**
   * Cancel navigation (stay on page)
   */
  const handleCancelNavigation = () => {
    setShowUnsavedModal(false);
    setPendingNavigation(null);
  };

  // Show skeleton while loading
  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8 max-w-4xl">
        <div className="mb-6">
          <div className="h-8 bg-gray-200 rounded w-48 mb-2 animate-pulse"></div>
          <div className="h-4 bg-gray-200 rounded w-96 animate-pulse"></div>
        </div>
        <FormSkeleton />
      </div>
    );
  }

  // Don't render form if no event (will redirect to 404)
  if (!event) {
    return null;
  }

  return (
    <>
      <div className="container mx-auto px-4 py-8 max-w-4xl">
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900">Edit Event</h1>
          <p className="text-gray-600 mt-2">Update the details for your volunteer event</p>
        </div>

        <div className="bg-white rounded-lg shadow-md p-6">
          <EventForm
            initialData={eventToFormData(event)}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
            submitLabel="Save Changes"
            isLoading={updateEvent.isPending}
            onChange={() => setHasUnsavedChanges(true)}
            isEditMode={true}
            existingEventDate={event.startTime}
          />
        </div>
      </div>

      <UnsavedChangesModal
        isOpen={showUnsavedModal}
        onConfirm={handleConfirmNavigation}
        onCancel={handleCancelNavigation}
      />
    </>
  );
}
