import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createEvent, type CreateEventRequest } from '@/services/eventService'
import type { EventFormData } from '@/components/events/forms/EventForm'

/**
 * Hook to create a new event
 * Invalidates event queries on success
 */
export function useCreateEvent() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (formData: EventFormData) => {
      // Combine date and time into ISO 8601 datetime
      const startTime = `${formData.date}T${formData.time}:00`

      // Combine registration deadline if provided
      let registrationDeadline: string | undefined
      if (formData.registrationDeadlineDate) {
        registrationDeadline = `${formData.registrationDeadlineDate}T${formData.registrationDeadlineTime || '00:00'}:00`
      }

      // Prepare request data
      const requestData: CreateEventRequest = {
        title: formData.title,
        description: formData.description,
        location: formData.location,
        startTime,
        durationMinutes: formData.durationMinutes,
        capacity: formData.capacity,
        registrationDeadline,
        requiredSkillIds: formData.requiredSkills.map(skill => skill.id),
        // imageUrl will be added when image upload API is implemented
        // For now, we can use a mock placeholder or leave it undefined
      }

      // TODO: When image upload API is ready, upload image first and get URL
      // if (formData.imageFile) {
      //   const imageUrl = await uploadEventImage(formData.imageFile)
      //   requestData.imageUrl = imageUrl
      // }

      return createEvent(requestData)
    },
    onSuccess: () => {
      // Invalidate and refetch event queries
      queryClient.invalidateQueries({ queryKey: ['events'] })
    },
  })
}
