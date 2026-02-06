import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { RegistrationCard } from '@/components/registrations/RegistrationCard';
import type { RegistrationResponse } from '@/services/registrationService';
import { EventStatus } from '@/types/enums';

const mockRegistration: RegistrationResponse = {
  id: 1,
  eventId: 10,
  userId: 5,
  status: 'Confirmed',
  registeredAt: '2026-02-01T10:00:00Z',
  event: {
    id: 10,
    title: 'Beach Cleanup',
    location: 'Sunny Beach, CA',
    startTime: '2026-03-15T09:00:00Z',
    durationMinutes: 180,
    status: EventStatus.Active,
    imageUrl: undefined,
  },
};

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('RegistrationCard', () => {
  it('renders event details correctly', () => {
    renderWithRouter(<RegistrationCard registration={mockRegistration} />);

    expect(screen.getByText('Beach Cleanup')).toBeInTheDocument();
    expect(screen.getByText(/Sunny Beach, CA/)).toBeInTheDocument();
    expect(screen.getByText('Confirmed')).toBeInTheDocument();
  });

  it('shows cancel button when showCancelButton is true', () => {
    renderWithRouter(
      <RegistrationCard
        registration={mockRegistration}
        showCancelButton={true}
        onCancelClick={vi.fn()}
      />
    );

    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
  });

  it('does not show cancel button when showCancelButton is false', () => {
    renderWithRouter(<RegistrationCard registration={mockRegistration} showCancelButton={false} />);

    expect(screen.queryByRole('button', { name: /cancel/i })).not.toBeInTheDocument();
  });

  it('calls onCancelClick when cancel button is clicked', () => {
    const onCancelClick = vi.fn();

    renderWithRouter(
      <RegistrationCard
        registration={mockRegistration}
        showCancelButton={true}
        onCancelClick={onCancelClick}
      />
    );

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    fireEvent.click(cancelButton);

    expect(onCancelClick).toHaveBeenCalledTimes(1);
  });

  it('shows "Event Cancelled" badge when event status is cancelled', () => {
    const cancelledRegistration = {
      ...mockRegistration,
      event: { ...mockRegistration.event, status: EventStatus.Cancelled },
    };

    renderWithRouter(<RegistrationCard registration={cancelledRegistration} />);

    expect(screen.getByText('Event Cancelled')).toBeInTheDocument();
  });

  it('does not show cancel button for cancelled events', () => {
    const cancelledRegistration = {
      ...mockRegistration,
      event: { ...mockRegistration.event, status: EventStatus.Cancelled },
    };

    renderWithRouter(
      <RegistrationCard
        registration={cancelledRegistration}
        showCancelButton={true}
        onCancelClick={vi.fn()}
      />
    );

    expect(screen.queryByRole('button', { name: /cancel/i })).not.toBeInTheDocument();
  });

  it('renders event image when imageUrl is provided', () => {
    const registrationWithImage = {
      ...mockRegistration,
      event: { ...mockRegistration.event, imageUrl: 'https://example.com/image.jpg' },
    };

    renderWithRouter(<RegistrationCard registration={registrationWithImage} />);

    const image = screen.getByAltText('Beach Cleanup');
    expect(image).toBeInTheDocument();
    expect(image).toHaveAttribute('src', 'https://example.com/image.jpg');
  });

  it('renders placeholder when no imageUrl', () => {
    renderWithRouter(<RegistrationCard registration={mockRegistration} />);

    // Check for SVG placeholder icon
    const links = screen.getAllByRole('link');
    const placeholder = links[0].querySelector('svg');
    expect(placeholder).toBeInTheDocument();
  });

  it('links to event details page', () => {
    renderWithRouter(<RegistrationCard registration={mockRegistration} />);

    const links = screen.getAllByRole('link');
    expect(links[0]).toHaveAttribute('href', '/events/10');
  });
});
