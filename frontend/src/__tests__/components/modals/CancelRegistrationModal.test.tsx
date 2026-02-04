import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { CancelRegistrationModal } from '@/components/modals/CancelRegistrationModal';
import { EventSummary } from '@/services/registrationService';
import { EventStatus } from '@/types/enums';

const mockEvent: EventSummary = {
  id: 10,
  title: 'Beach Cleanup',
  location: 'Sunny Beach, CA',
  startTime: '2026-03-15T09:00:00Z',
  durationMinutes: 180,
  status: EventStatus.Active,
  imageUrl: null,
};

describe('CancelRegistrationModal', () => {
  it('does not render when isOpen is false', () => {
    const { container } = render(
      <CancelRegistrationModal
        event={mockEvent}
        isOpen={false}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    expect(container).toBeEmptyDOMElement();
  });

  it('renders modal with event details when isOpen is true', () => {
    render(
      <CancelRegistrationModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    expect(screen.getByRole('heading', { name: 'Cancel Registration' })).toBeInTheDocument();
    expect(screen.getByText('Beach Cleanup')).toBeInTheDocument();
    expect(screen.getByText(/Sunny Beach, CA/)).toBeInTheDocument();
  });

  it('shows warning message about action being irreversible', () => {
    render(
      <CancelRegistrationModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    expect(screen.getByText(/This action cannot be undone/)).toBeInTheDocument();
  });

  it('calls onClose when "Keep Registration" button is clicked', () => {
    const onClose = vi.fn();

    render(
      <CancelRegistrationModal
        event={mockEvent}
        isOpen={true}
        onClose={onClose}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    const keepButton = screen.getByRole('button', { name: /keep registration/i });
    fireEvent.click(keepButton);

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('calls onConfirm when "Cancel Registration" button is clicked', () => {
    const onConfirm = vi.fn();

    render(
      <CancelRegistrationModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={onConfirm}
        isLoading={false}
      />
    );

    const cancelButton = screen.getByRole('button', { name: /^cancel registration$/i });
    fireEvent.click(cancelButton);

    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it('disables buttons and shows loading state when isLoading is true', () => {
    render(
      <CancelRegistrationModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={true}
      />
    );

    const keepButton = screen.getByRole('button', { name: /keep registration/i });
    const cancelButton = screen.getByRole('button', { name: /cancelling/i });

    expect(keepButton).toBeDisabled();
    expect(cancelButton).toBeDisabled();
    expect(screen.getByText('Cancelling...')).toBeInTheDocument();
  });

  it('calls onClose when close icon is clicked', () => {
    const onClose = vi.fn();

    render(
      <CancelRegistrationModal
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

  it('displays formatted date and time', () => {
    render(
      <CancelRegistrationModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    // Check for formatted date and time (time may vary by timezone)
    expect(screen.getByText(/Sunday, March 15, 2026 at \d{1,2}:\d{2} (AM|PM)/)).toBeInTheDocument();
  });

  it('displays duration correctly', () => {
    render(
      <CancelRegistrationModal
        event={mockEvent}
        isOpen={true}
        onClose={vi.fn()}
        onConfirm={vi.fn()}
        isLoading={false}
      />
    );

    expect(screen.getByText('3 hours')).toBeInTheDocument();
  });
});
