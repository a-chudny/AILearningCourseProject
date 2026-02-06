import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { EventForm, type EventFormData } from '@/components/events/forms/EventForm';

// Mock skillService
vi.mock('@/services/skillService', () => ({
  getSkills: vi.fn().mockResolvedValue([
    {
      id: 1,
      name: 'First Aid',
      description: 'Medical & Healthcare',
      createdAt: '2026-01-01T00:00:00Z',
    },
    { id: 2, name: 'Teaching', description: 'Education', createdAt: '2026-01-01T00:00:00Z' },
  ]),
}));

describe('EventForm', () => {
  const mockOnSubmit = vi.fn();
  const mockOnCancel = vi.fn();

  beforeEach(() => {
    mockOnSubmit.mockClear();
    mockOnCancel.mockClear();
  });

  it('renders all required form fields', async () => {
    render(<EventForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

    await waitFor(() => {
      expect(screen.getByLabelText(/Event Title/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/Description/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/Event Date/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/Start Time/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/Duration/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/Location/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/Volunteer Capacity/i)).toBeInTheDocument();
    });
  });

  it('shows validation errors for empty required fields on submit', async () => {
    render(<EventForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

    const submitButton = screen.getByRole('button', { name: /Create Event/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Title is required')).toBeInTheDocument();
      expect(screen.getByText('Description is required')).toBeInTheDocument();
      expect(screen.getByText('Location is required')).toBeInTheDocument();
      expect(screen.getByText('Event date is required')).toBeInTheDocument();
      expect(screen.getByText('Event time is required')).toBeInTheDocument();
    });

    expect(mockOnSubmit).not.toHaveBeenCalled();
  });

  it('validates title max length', async () => {
    render(<EventForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

    const titleInput = screen.getByLabelText(/Event Title/i);
    const longTitle = 'a'.repeat(201);

    fireEvent.change(titleInput, { target: { value: longTitle } });
    fireEvent.blur(titleInput);

    await waitFor(() => {
      expect(screen.getByText('Title must be 200 characters or less')).toBeInTheDocument();
    });
  });

  it('validates future date requirement', async () => {
    render(<EventForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

    const dateInput = screen.getByLabelText(/Event Date/i);
    const pastDate = '2020-01-01';

    fireEvent.change(dateInput, { target: { value: pastDate } });
    fireEvent.blur(dateInput);

    await waitFor(() => {
      expect(screen.getByText('Event date must be in the future')).toBeInTheDocument();
    });
  });

  it('handles duration selection', async () => {
    render(<EventForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

    const durationSelect = screen.getByLabelText(/Duration/i) as HTMLSelectElement;

    // Default should be 2 hours (120)
    expect(durationSelect.value).toBe('120');

    // Select 4 hours
    fireEvent.change(durationSelect, { target: { value: '240' } });

    await waitFor(() => {
      expect(durationSelect.value).toBe('240');
    });
  });

  it('allows skill selection', async () => {
    render(<EventForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

    await waitFor(() => {
      const skillButton = screen.getByText('Select skills');
      expect(skillButton).toBeInTheDocument();
    });

    const skillButton = screen.getByText('Select skills');
    fireEvent.click(skillButton);

    await waitFor(() => {
      expect(screen.getByText('First Aid')).toBeInTheDocument();
      expect(screen.getByText('Teaching')).toBeInTheDocument();
    });

    const firstAidSkill = screen.getByText('First Aid');
    fireEvent.click(firstAidSkill);

    await waitFor(() => {
      // Check for skill chip
      const skillChips = screen.getAllByText('First Aid');
      expect(skillChips.length).toBeGreaterThan(1);
    });
  });

  it('calls onCancel when cancel button is clicked', () => {
    render(<EventForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

    const cancelButton = screen.getByRole('button', { name: /Cancel/i });
    fireEvent.click(cancelButton);

    expect(mockOnCancel).toHaveBeenCalledTimes(1);
  });

  it('shows loading state during submission', async () => {
    render(<EventForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} isLoading={true} />);

    expect(screen.getByText('Submitting...')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Submitting/i })).toBeDisabled();
  });

  it('displays character count for title and description', async () => {
    render(<EventForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

    expect(screen.getByText('0/200 characters')).toBeInTheDocument();
    expect(screen.getByText('0/2000 characters')).toBeInTheDocument();

    const titleInput = screen.getByLabelText(/Event Title/i);
    fireEvent.change(titleInput, { target: { value: 'Test Event' } });

    await waitFor(() => {
      expect(screen.getByText('10/200 characters')).toBeInTheDocument();
    });
  });

  it('populates form with initial data', async () => {
    const initialData: Partial<EventFormData> = {
      title: 'Test Event',
      description: 'Test Description',
      location: 'Test Location',
      capacity: 50,
    };

    render(<EventForm initialData={initialData} onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

    expect(screen.getByLabelText(/Event Title/i)).toHaveValue('Test Event');
    expect(screen.getByLabelText(/Description/i)).toHaveValue('Test Description');
    expect(screen.getByLabelText(/Location/i)).toHaveValue('Test Location');
    expect(screen.getByLabelText(/Volunteer Capacity/i)).toHaveValue(50);
  });
});
