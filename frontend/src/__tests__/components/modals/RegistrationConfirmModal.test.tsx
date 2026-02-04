import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { RegistrationConfirmModal } from '@/components/modals/RegistrationConfirmModal';
import type { EventResponse } from '@/services/eventService';

const mockEvent: EventResponse = {
  id: 1,
  title: 'Beach Cleanup',
  description: 'Help us clean up the local beach',
  startTime: new Date('2026-03-15T09:00:00').toISOString(),
  durationMinutes: 180,
  location: '123 Beach St, San Diego, CA',
  capacity: 20,
  registrationCount: 15,
  status: 'Active',
  organizerId: 2,
  organizerName: 'John Doe',
  requiredSkills: [
    { id: 1, name: 'Teamwork', description: 'Work well in a team', createdAt: new Date().toISOString() },
    { id: 2, name: 'Physical Fitness', description: 'Able to do physical work', createdAt: new Date().toISOString() },
  ],
  imageUrl: undefined,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
};

describe('RegistrationConfirmModal', () => {
  it('should not render when isOpen is false', () => {
    const { container } = render(
      <RegistrationConfirmModal
        event={mockEvent}
        isOpen={false}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    expect(container).toBeEmptyDOMElement();
  });

  it('should render modal with event details when isOpen is true', () => {
    render(
      <RegistrationConfirmModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    expect(screen.getByRole('heading', { name: 'Confirm Registration' })).toBeInTheDocument();
    expect(screen.getByText('Beach Cleanup')).toBeInTheDocument();
    expect(screen.getByText(/Organized by John Doe/)).toBeInTheDocument();
    expect(screen.getByText(/123 Beach St, San Diego, CA/)).toBeInTheDocument();
    expect(screen.getByText(/15 \/ 20 registered/)).toBeInTheDocument();
  });

  it('should display required skills', () => {
    render(
      <RegistrationConfirmModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    expect(screen.getByText('Teamwork')).toBeInTheDocument();
    expect(screen.getByText('Physical Fitness')).toBeInTheDocument();
  });

  it('should call onClose when Cancel button is clicked', () => {
    const onClose = vi.fn();

    render(
      <RegistrationConfirmModal
        event={mockEvent}
        isOpen={true}
        onClose={onClose}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    fireEvent.click(cancelButton);

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('should call onConfirm when Confirm button is clicked', () => {
    const onConfirm = vi.fn();

    render(
      <RegistrationConfirmModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={onConfirm}
        isLoading={false}
      />
    );

    const confirmButton = screen.getByRole('button', { name: /confirm registration/i });
    fireEvent.click(confirmButton);

    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it('should disable buttons and show loading state when isLoading is true', () => {
    render(
      <RegistrationConfirmModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={true}
      />
    );

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    const confirmButton = screen.getByRole('button', { name: /confirming/i });

    expect(cancelButton).toBeDisabled();
    expect(confirmButton).toBeDisabled();
    expect(screen.getByText('Confirming...')).toBeInTheDocument();
  });

  it('should call onClose when close icon is clicked', () => {
    const onClose = vi.fn();

    render(
      <RegistrationConfirmModal
        event={mockEvent}
        isOpen={true}
        onClose={onClose}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    const closeButton = screen.getByLabelText(/close modal/i);
    fireEvent.click(closeButton);

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('should show available spots correctly', () => {
    render(
      <RegistrationConfirmModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    expect(screen.getByText(/\(5 spots left\)/)).toBeInTheDocument();
  });

  it('should render without required skills when event has none', () => {
    const eventWithoutSkills = { ...mockEvent, requiredSkills: [] };

    render(
      <RegistrationConfirmModal
        event={eventWithoutSkills}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    expect(screen.queryByText('Required Skills')).not.toBeInTheDocument();
  });
});
