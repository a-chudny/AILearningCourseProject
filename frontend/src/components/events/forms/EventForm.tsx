import { useState, useEffect, useRef, type FormEvent, type ChangeEvent } from 'react';
import type { Skill } from '@/types';
import { getSkills } from '@/services/skillService';
import { ImageUpload } from '@/components/ImageUpload';

export interface EventFormData {
  title: string;
  description: string;
  location: string;
  date: string;
  time: string;
  durationMinutes: number;
  customDuration: string;
  capacity: number;
  imageFile: File | null;
  imagePreview: string;
  registrationDeadlineDate: string;
  registrationDeadlineTime: string;
  requiredSkills: Skill[];
}

export interface EventFormErrors {
  title?: string;
  description?: string;
  location?: string;
  date?: string;
  time?: string;
  durationMinutes?: string;
  capacity?: string;
  registrationDeadline?: string;
  submit?: string;
}

interface EventFormProps {
  initialData?: Partial<EventFormData>;
  onSubmit: (data: EventFormData) => Promise<void>;
  onCancel: () => void;
  submitLabel?: string;
  isLoading?: boolean;
  onChange?: () => void;
  isEditMode?: boolean;
  existingEventDate?: string;
}

const DURATION_PRESETS = [
  { label: '1 hour', value: 60 },
  { label: '2 hours', value: 120 },
  { label: '4 hours', value: 240 },
  { label: '8 hours', value: 480 },
  { label: 'Custom', value: 0 },
];

export function EventForm({
  initialData,
  onSubmit,
  onCancel,
  submitLabel = 'Create Event',
  isLoading = false,
  onChange,
  isEditMode = false,
  existingEventDate,
}: EventFormProps) {
  const [availableSkills, setAvailableSkills] = useState<Skill[]>([]);
  const [isLoadingSkills, setIsLoadingSkills] = useState(true);
  const [isSkillDropdownOpen, setIsSkillDropdownOpen] = useState(false);
  const skillDropdownRef = useRef<HTMLDivElement>(null);
  const timeInputRefs = useRef<{ time?: HTMLInputElement; deadlineTime?: HTMLInputElement }>({});
  const timeClickTimerRef = useRef<{ time?: number; deadlineTime?: number }>({});
  const [timeInputMode, setTimeInputMode] = useState<{
    time: 'picker' | 'manual';
    deadlineTime: 'picker' | 'manual';
  }>({ time: 'picker', deadlineTime: 'picker' });

  const [formData, setFormData] = useState<EventFormData>({
    title: '',
    description: '',
    location: '',
    date: '',
    time: '',
    durationMinutes: 120, // Default 2 hours
    customDuration: '',
    capacity: 10,
    imageFile: null,
    imagePreview: '',
    registrationDeadlineDate: '',
    registrationDeadlineTime: '',
    requiredSkills: [],
    ...initialData,
  });

  const [errors, setErrors] = useState<EventFormErrors>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});

  // Load available skills
  useEffect(() => {
    const loadSkills = async () => {
      try {
        const skills = await getSkills();
        setAvailableSkills(skills);
      } catch (error) {
        console.error('Failed to load skills:', error);
      } finally {
        setIsLoadingSkills(false);
      }
    };
    loadSkills();
  }, []);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (skillDropdownRef.current && !skillDropdownRef.current.contains(event.target as Node)) {
        setIsSkillDropdownOpen(false);
      }
    };

    if (isSkillDropdownOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      return () => {
        document.removeEventListener('mousedown', handleClickOutside);
      };
    }
  }, [isSkillDropdownOpen]);

  // Check if event date is in the past (for edit mode)
  const isEventDateInPast = () => {
    if (!isEditMode || !existingEventDate) return false;
    const existingDate = new Date(existingEventDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return existingDate < today;
  };

  // Validate single field
  const validateField = (
    name: keyof EventFormData,
    value: EventFormData[keyof EventFormData]
  ): string | undefined => {
    switch (name) {
      case 'title':
        if (!value || value.trim().length === 0) return 'Title is required';
        if (value.length > 200) return 'Title must be 200 characters or less';
        break;

      case 'description':
        if (!value || value.trim().length === 0) return 'Description is required';
        if (value.length > 2000) return 'Description must be 2000 characters or less';
        break;

      case 'location':
        if (!value || value.trim().length === 0) return 'Location is required';
        if (value.length > 300) return 'Location must be 300 characters or less';
        break;

      case 'date': {
        if (!value) return 'Event date is required';
        const selectedDate = new Date(value);
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        // In edit mode, allow existing past event dates (field will be locked)
        if (isEditMode && existingEventDate) {
          const existingDate = new Date(existingEventDate);
          existingDate.setHours(0, 0, 0, 0);

          // If existing event is in the past, date field is locked at that value
          if (existingDate < today) {
            break; // Allow the existing past date
          }
        }

        if (selectedDate < today) return 'Event date must be in the future';
        break;
      }

      case 'time':
        if (!value) return 'Event time is required';
        break;

      case 'durationMinutes':
        // When duration is 0, it means Custom is selected - check customDuration instead
        if (value === 0) {
          const customValue = parseInt(formData.customDuration, 10);
          if (!customValue || customValue <= 0) return 'Duration must be greater than 0';
          if (customValue > 1440) return 'Duration cannot exceed 24 hours (1440 minutes)';
        } else {
          if (!value || value <= 0) return 'Duration must be greater than 0';
          if (value > 1440) return 'Duration cannot exceed 24 hours (1440 minutes)';
        }
        break;

      case 'capacity':
        if (!value || value < 1) return 'Capacity must be at least 1';
        if (value > 10000) return 'Capacity seems unrealistically high';
        break;
    }

    // Validate registration deadline if provided
    if (formData.registrationDeadlineDate) {
      // Require time when date is provided
      if (!formData.registrationDeadlineTime) {
        if (name === 'registrationDeadlineDate' || name === 'registrationDeadlineTime') {
          return 'Registration deadline time is required';
        }
      }

      // Check if deadline is before event start
      if (formData.date && formData.registrationDeadlineTime) {
        const deadline = new Date(
          `${formData.registrationDeadlineDate}T${formData.registrationDeadlineTime}`
        );
        const eventDate = new Date(`${formData.date}T${formData.time || '00:00'}`);
        if (deadline >= eventDate) {
          if (name === 'registrationDeadlineDate' || name === 'registrationDeadlineTime') {
            return 'Registration deadline must be before event start time';
          }
        }
      }
    }

    return undefined;
  };

  // Handle time input click - single click opens picker, double click allows manual entry
  const handleTimeInputClick = (
    field: 'time' | 'deadlineTime',
    e: React.MouseEvent<HTMLInputElement>
  ) => {
    const input = timeInputRefs.current[field];
    if (!input) return;

    // If already in manual mode, just let the click pass through
    if (timeInputMode[field] === 'manual') {
      return;
    }

    if (timeClickTimerRef.current[field]) {
      // Double click detected - switch to manual entry mode
      clearTimeout(timeClickTimerRef.current[field]!);
      timeClickTimerRef.current[field] = undefined;
      setTimeInputMode((prev) => ({ ...prev, [field]: 'manual' }));
      // Focus and select all text for easy replacement
      setTimeout(() => {
        input.focus();
        input.select();
      }, 0);
    } else {
      // First click - start timer and open picker
      e.preventDefault();
      timeClickTimerRef.current[field] = window.setTimeout(() => {
        timeClickTimerRef.current[field] = undefined;
        // Single click confirmed - open picker
        if ('showPicker' in HTMLInputElement.prototype) {
          try {
            (input as HTMLInputElement).showPicker();
          } catch {
            input.focus();
          }
        } else {
          input.focus();
        }
      }, 250);
    }
  };

  // Reset time input mode on blur
  const handleTimeInputBlur = (field: 'time' | 'deadlineTime') => {
    // Reset to picker mode after a delay (allows for picker interaction)
    setTimeout(() => {
      setTimeInputMode((prev) => ({ ...prev, [field]: 'picker' }));
    }, 100);
  };

  // Handle input change with real-time validation
  const handleChange = (
    e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;

    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));

    // Real-time validation
    if (touched[name]) {
      const error = validateField(name as keyof EventFormData, value);
      setErrors((prev) => ({
        ...prev,
        [name]: error,
      }));
    }

    // Notify parent of changes
    onChange?.();
  };

  // Handle number input change
  const handleNumberChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    const numValue = parseInt(value, 10);

    setFormData((prev) => ({
      ...prev,
      [name]: isNaN(numValue) ? '' : numValue,
    }));

    if (touched[name]) {
      const error = validateField(name as keyof EventFormData, isNaN(numValue) ? '' : numValue);
      setErrors((prev) => ({
        ...prev,
        [name]: error,
      }));
    }

    // Notify parent of changes
    onChange?.();
  };

  // Handle duration preset change
  const handleDurationChange = (e: ChangeEvent<HTMLSelectElement>) => {
    const value = parseInt(e.target.value, 10);

    if (value === 0) {
      // Custom duration selected - keep durationMinutes at 0 so dropdown stays on Custom
      setFormData((prev) => ({
        ...prev,
        durationMinutes: 0,
      }));
    } else {
      setFormData((prev) => ({
        ...prev,
        durationMinutes: value,
        customDuration: '',
      }));
    }

    if (touched.durationMinutes) {
      const error = validateField('durationMinutes', value === 0 ? formData.customDuration : value);
      setErrors((prev) => ({
        ...prev,
        durationMinutes: error,
      }));
    }

    // Notify parent of changes
    onChange?.();
  };

  // Handle custom duration input
  const handleCustomDurationChange = (e: ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    const numValue = parseInt(value, 10);

    setFormData((prev) => ({
      ...prev,
      customDuration: value,
      durationMinutes: isNaN(numValue) ? 0 : numValue,
    }));

    if (touched.durationMinutes) {
      const error = validateField('durationMinutes', isNaN(numValue) ? 0 : numValue);
      setErrors((prev) => ({
        ...prev,
        durationMinutes: error,
      }));
    }

    // Notify parent of changes
    onChange?.();
  };

  // Remove selected image
  const handleRemoveImage = () => {
    setFormData((prev) => ({
      ...prev,
      imageFile: null,
      imagePreview: '',
    }));
    // Notify parent of changes
    onChange?.();
  };

  // Toggle skill selection
  const toggleSkill = (skill: Skill) => {
    setFormData((prev) => {
      const isSelected = prev.requiredSkills.some((s) => s.id === skill.id);
      return {
        ...prev,
        requiredSkills: isSelected
          ? prev.requiredSkills.filter((s) => s.id !== skill.id)
          : [...prev.requiredSkills, skill],
      };
    });
    // Notify parent of changes
    onChange?.();
  };

  // Remove skill chip
  const removeSkill = (skillId: number) => {
    setFormData((prev) => ({
      ...prev,
      requiredSkills: prev.requiredSkills.filter((s) => s.id !== skillId),
    }));
    // Notify parent of changes
    onChange?.();
  };

  // Handle blur for touched state
  const handleBlur = (
    e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name } = e.target;
    setTouched((prev) => ({
      ...prev,
      [name]: true,
    }));

    const value = formData[name as keyof EventFormData];
    const error = validateField(name as keyof EventFormData, value);
    setErrors((prev) => ({
      ...prev,
      [name]: error,
    }));
  };

  // Validate all fields
  const validateForm = (): boolean => {
    const newErrors: EventFormErrors = {};
    let isValid = true;

    // Validate all required fields
    const titleError = validateField('title', formData.title);
    if (titleError) {
      newErrors.title = titleError;
      isValid = false;
    }

    const descError = validateField('description', formData.description);
    if (descError) {
      newErrors.description = descError;
      isValid = false;
    }

    const locError = validateField('location', formData.location);
    if (locError) {
      newErrors.location = locError;
      isValid = false;
    }

    const dateError = validateField('date', formData.date);
    if (dateError) {
      newErrors.date = dateError;
      isValid = false;
    }

    const timeError = validateField('time', formData.time);
    if (timeError) {
      newErrors.time = timeError;
      isValid = false;
    }

    const durationError = validateField('durationMinutes', formData.durationMinutes);
    if (durationError) {
      newErrors.durationMinutes = durationError;
      isValid = false;
    }

    const capacityError = validateField('capacity', formData.capacity);
    if (capacityError) {
      newErrors.capacity = capacityError;
      isValid = false;
    }

    // Validate registration deadline if provided
    if (formData.registrationDeadlineDate) {
      const deadline = new Date(
        `${formData.registrationDeadlineDate}T${formData.registrationDeadlineTime || '00:00'}`
      );
      const eventDate = new Date(`${formData.date}T${formData.time || '00:00'}`);
      if (deadline >= eventDate) {
        newErrors.registrationDeadline = 'Registration deadline must be before event start time';
        isValid = false;
      }
    }

    setErrors(newErrors);
    return isValid;
  };

  // Handle form submission
  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Mark all fields as touched
    setTouched({
      title: true,
      description: true,
      location: true,
      date: true,
      time: true,
      durationMinutes: true,
      capacity: true,
      registrationDeadlineDate: true,
      registrationDeadlineTime: true,
    });

    // Validate form
    if (!validateForm()) {
      // Scroll to first invalid field
      const firstErrorField = Object.keys(errors).find((key) => errors[key as keyof typeof errors]);
      if (firstErrorField) {
        const element =
          document.getElementById(firstErrorField) ||
          document.querySelector(`[name="${firstErrorField}"]`);
        if (element) {
          element.scrollIntoView({ behavior: 'smooth', block: 'center' });
          element.focus();
        }
      }
      return;
    }

    try {
      await onSubmit(formData);
    } catch (error: unknown) {
      const errorMessage =
        error instanceof Error ? error.message : 'Failed to submit form. Please try again.';
      setErrors((prev) => ({
        ...prev,
        submit: errorMessage,
      }));
    }
  };

  const isCustomDuration = !DURATION_PRESETS.slice(0, -1).some(
    (p) => p.value === formData.durationMinutes
  );

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Title */}
      <div>
        <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-1">
          Event Title <span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          id="title"
          name="title"
          value={formData.title}
          onChange={handleChange}
          onBlur={handleBlur}
          maxLength={200}
          className={`w-full px-3 py-2 border rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.title && touched.title ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="e.g., Beach Cleanup Day"
          disabled={isLoading}
        />
        {errors.title && touched.title && (
          <p className="mt-1 text-sm text-red-600">{errors.title}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">{formData.title.length}/200 characters</p>
      </div>

      {/* Description */}
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
          Description <span className="text-red-500">*</span>
        </label>
        <textarea
          id="description"
          name="description"
          value={formData.description}
          onChange={handleChange}
          onBlur={handleBlur}
          maxLength={2000}
          rows={6}
          className={`w-full px-3 py-2 border rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.description && touched.description ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="Describe the event, activities, and what volunteers will do..."
          disabled={isLoading}
        />
        {errors.description && touched.description && (
          <p className="mt-1 text-sm text-red-600">{errors.description}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">{formData.description.length}/2000 characters</p>
      </div>

      {/* Date and Time Row */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
        {/* Date */}
        <div>
          <label htmlFor="date" className="block text-sm font-medium text-gray-700 mb-1">
            Event Date <span className="text-red-500">*</span>
            {isEventDateInPast() && (
              <span className="ml-2 text-xs text-gray-500">(Past event - locked)</span>
            )}
          </label>
          <input
            type="date"
            id="date"
            name="date"
            value={formData.date}
            onChange={handleChange}
            onBlur={handleBlur}
            className={`w-full px-3 py-2 border rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 ${
              errors.date && touched.date ? 'border-red-500' : 'border-gray-300'
            } ${isEventDateInPast() ? 'bg-gray-100 cursor-not-allowed' : ''}`}
            disabled={isLoading || isEventDateInPast()}
          />
          {errors.date && touched.date && (
            <p className="mt-1 text-sm text-red-600">{errors.date}</p>
          )}
        </div>

        {/* Time */}
        <div>
          <label htmlFor="time" className="block text-sm font-medium text-gray-700 mb-1">
            Start Time <span className="text-red-500">*</span>
          </label>
          <input
            type="time"
            id="time"
            name="time"
            value={formData.time}
            onChange={handleChange}
            onBlur={(e) => {
              handleBlur(e);
              handleTimeInputBlur('time');
            }}
            onClick={(e) => handleTimeInputClick('time', e)}
            ref={(el) => {
              if (el) timeInputRefs.current.time = el;
            }}
            className={`w-full px-3 py-2 border rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 ${
              errors.time && touched.time ? 'border-red-500' : 'border-gray-300'
            }`}
            disabled={isLoading}
          />
          {errors.time && touched.time && (
            <p className="mt-1 text-sm text-red-600">{errors.time}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            Single click: time picker, Double click: manual entry
          </p>
        </div>
      </div>

      {/* Duration */}
      <div>
        <label htmlFor="duration" className="block text-sm font-medium text-gray-700 mb-1">
          Duration <span className="text-red-500">*</span>
        </label>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <select
            id="duration"
            value={isCustomDuration ? 0 : formData.durationMinutes}
            onChange={handleDurationChange}
            className={`w-full px-3 py-2 border rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 ${
              errors.durationMinutes && touched.durationMinutes
                ? 'border-red-500'
                : 'border-gray-300'
            }`}
            disabled={isLoading}
          >
            {DURATION_PRESETS.map((preset) => (
              <option key={preset.value} value={preset.value}>
                {preset.label}
              </option>
            ))}
          </select>

          {isCustomDuration && (
            <div className="flex items-center gap-2">
              <input
                type="number"
                value={formData.customDuration}
                onChange={handleCustomDurationChange}
                onBlur={handleBlur}
                min={1}
                max={1440}
                className="flex-1 px-3 py-2 border border-gray-300 rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Minutes"
                disabled={isLoading}
              />
              <span className="text-sm text-gray-500">minutes</span>
            </div>
          )}
        </div>
        {errors.durationMinutes && touched.durationMinutes && (
          <p className="mt-1 text-sm text-red-600">{errors.durationMinutes}</p>
        )}
      </div>

      {/* Location */}
      <div>
        <label htmlFor="location" className="block text-sm font-medium text-gray-700 mb-1">
          Location <span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          id="location"
          name="location"
          value={formData.location}
          onChange={handleChange}
          onBlur={handleBlur}
          maxLength={300}
          className={`w-full px-3 py-2 border rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.location && touched.location ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="e.g., Santa Monica Beach, Los Angeles, CA"
          disabled={isLoading}
        />
        {errors.location && touched.location && (
          <p className="mt-1 text-sm text-red-600">{errors.location}</p>
        )}
      </div>

      {/* Capacity */}
      <div>
        <label htmlFor="capacity" className="block text-sm font-medium text-gray-700 mb-1">
          Volunteer Capacity <span className="text-red-500">*</span>
        </label>
        <input
          type="number"
          id="capacity"
          name="capacity"
          value={formData.capacity}
          onChange={handleNumberChange}
          onBlur={handleBlur}
          min={1}
          max={10000}
          className={`w-full px-3 py-2 border rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.capacity && touched.capacity ? 'border-red-500' : 'border-gray-300'
          }`}
          disabled={isLoading}
        />
        {errors.capacity && touched.capacity && (
          <p className="mt-1 text-sm text-red-600">{errors.capacity}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">Maximum number of volunteers needed</p>
      </div>

      {/* Registration Deadline (Optional) */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Registration Deadline (Optional)
        </label>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <input
            type="date"
            name="registrationDeadlineDate"
            value={formData.registrationDeadlineDate}
            onChange={handleChange}
            onBlur={handleBlur}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={isLoading}
          />
          <input
            type="time"
            name="registrationDeadlineTime"
            value={formData.registrationDeadlineTime}
            onChange={handleChange}
            onBlur={(e) => {
              handleBlur(e);
              handleTimeInputBlur('deadlineTime');
            }}
            onClick={(e) => handleTimeInputClick('deadlineTime', e)}
            ref={(el) => {
              if (el) timeInputRefs.current.deadlineTime = el;
            }}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={isLoading || !formData.registrationDeadlineDate}
          />
        </div>
        {errors.registrationDeadline && (
          <p className="mt-1 text-sm text-red-600">{errors.registrationDeadline}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">
          Last date and time for volunteers to register (must be before event start)
        </p>
        <p className="mt-1 text-xs text-gray-500">
          Time: Single click for picker, Double click for manual entry
        </p>
      </div>

      {/* Event Image Upload (Optional) */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Event Image (Optional)
        </label>

        <ImageUpload
          imageUrl={formData.imagePreview}
          onImageSelect={(file) => {
            // Create preview
            const reader = new FileReader();
            reader.onloadend = () => {
              setFormData((prev) => ({
                ...prev,
                imageFile: file,
                imagePreview: reader.result as string,
              }));
              // Notify parent of changes
              onChange?.();
            };
            reader.readAsDataURL(file);
          }}
          onImageRemove={handleRemoveImage}
          isUploading={false}
          disabled={isLoading}
        />
      </div>

      {/* Required Skills (Optional) */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Required Skills (Optional)
        </label>

        {/* Selected skills chips */}
        {formData.requiredSkills.length > 0 && (
          <div className="flex flex-wrap gap-2 mb-3">
            {formData.requiredSkills.map((skill) => (
              <span
                key={skill.id}
                className="inline-flex items-center gap-1 px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm"
              >
                {skill.name}
                <button
                  type="button"
                  onClick={() => removeSkill(skill.id)}
                  className="hover:text-blue-600"
                  disabled={isLoading}
                >
                  <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>
              </span>
            ))}
          </div>
        )}

        {/* Skills dropdown */}
        <div className="relative" ref={skillDropdownRef}>
          <button
            type="button"
            onClick={() => setIsSkillDropdownOpen(!isSkillDropdownOpen)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm bg-white text-left focus:outline-none focus:ring-2 focus:ring-blue-500 flex items-center justify-between"
            disabled={isLoading || isLoadingSkills}
          >
            <span className="text-gray-700">
              {isLoadingSkills ? 'Loading skills...' : 'Select skills'}
            </span>
            <svg
              className={`h-5 w-5 text-gray-400 transition-transform ${isSkillDropdownOpen ? 'rotate-180' : ''}`}
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M19 9l-7 7-7-7"
              />
            </svg>
          </button>

          {isSkillDropdownOpen && !isLoadingSkills && (
            <div className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-auto">
              {availableSkills.map((skill) => {
                const isSelected = formData.requiredSkills.some((s) => s.id === skill.id);
                return (
                  <button
                    key={skill.id}
                    type="button"
                    onClick={() => toggleSkill(skill)}
                    className={`w-full text-left px-4 py-2 hover:bg-gray-50 flex items-center justify-between ${
                      isSelected ? 'bg-blue-50' : ''
                    }`}
                    disabled={isLoading}
                  >
                    <div>
                      <div className="font-medium text-gray-900">{skill.name}</div>
                      <div className="text-xs text-gray-500">{skill.category}</div>
                    </div>
                    {isSelected && (
                      <svg
                        className="h-5 w-5 text-blue-600"
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
                    )}
                  </button>
                );
              })}
            </div>
          )}
        </div>
        <p className="mt-1 text-sm text-gray-500">Select skills that volunteers should have</p>
      </div>

      {/* Error Message */}
      {errors.submit && (
        <div className="rounded-md bg-red-50 p-4">
          <div className="flex">
            <svg
              className="h-5 w-5 text-red-400"
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
            <p className="ml-3 text-sm text-red-700">{errors.submit}</p>
          </div>
        </div>
      )}

      {/* Form Actions */}
      <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          disabled={isLoading}
        >
          Cancel
        </button>
        <button
          type="submit"
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          disabled={isLoading}
        >
          {isLoading ? (
            <span className="flex items-center gap-2">
              <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle
                  className="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  strokeWidth="4"
                />
                <path
                  className="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                />
              </svg>
              Submitting...
            </span>
          ) : (
            submitLabel
          )}
        </button>
      </div>
    </form>
  );
}
