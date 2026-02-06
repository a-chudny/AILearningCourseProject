import { useNavigate } from 'react-router-dom';
import { EventForm, type EventFormData } from '@/components/events/forms/EventForm';
import { useCreateEvent } from '@/hooks/useCreateEvent';
import { uploadEventImage } from '@/services/imageService';

export default function CreateEventPage() {
  const navigate = useNavigate();
  const createEventMutation = useCreateEvent();

  const handleSubmit = async (formData: EventFormData) => {
    try {
      const createdEvent = await createEventMutation.mutateAsync(formData);

      // Upload image if one was selected
      if (formData.imageFile) {
        try {
          await uploadEventImage(createdEvent.id, formData.imageFile);
        } catch (imageError) {
          console.error('Failed to upload image:', imageError);
          // Don't fail the whole operation if just image upload fails
        }
      }

      // Redirect to My Events page (placeholder route)
      // TODO: Update route when My Events page is implemented
      navigate('/my-events', {
        state: {
          message: `Event "${createdEvent.title}" created successfully!`,
          eventId: createdEvent.id,
        },
      });
    } catch (error: any) {
      // Error will be handled by EventForm's error state
      throw new Error(error.response?.data?.message || 'Failed to create event');
    }
  };

  const handleCancel = () => {
    navigate(-1); // Go back to previous page
  };

  return (
    <div className="max-w-4xl mx-auto">
      <div className="bg-white rounded-lg shadow-md p-6 sm:p-8">
        {/* Page Header */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900">Create New Event</h1>
          <p className="mt-2 text-sm text-gray-600">
            Fill out the form below to create a new volunteer event. Fields marked with{' '}
            <span className="text-red-500">*</span> are required.
          </p>
        </div>

        {/* Event Form */}
        <EventForm
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          submitLabel="Create Event"
          isLoading={createEventMutation.isPending}
        />
      </div>
    </div>
  );
}
