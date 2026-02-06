import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { EventCard } from '@/components/events/EventCard';
import { EventStatus } from '@/types/enums';
import type { EventResponse } from '@/services/eventService';

// Helper to wrap component with Router
const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

const mockEvent: EventResponse = {
  id: 1,
  title: 'Beach Cleanup Event',
  description: 'Help clean up the local beach',
  location: '123 Beach Road, Coastal City',
  startTime: '2026-03-15T10:00:00Z',
  durationMinutes: 180,
  capacity: 50,
  imageUrl: 'https://example.com/beach.jpg',
  registrationDeadline: '2026-03-14T23:59:59Z',
  status: EventStatus.Active,
  organizerId: 2,
  organizerName: 'John Organizer',
  registrationCount: 25,
  requiredSkills: [
    {
      id: 1,
      name: 'First Aid',
      description: 'First aid skills',
      createdAt: '2026-01-01T00:00:00Z',
    },
    { id: 2, name: 'Driving', description: 'Driving skills', createdAt: '2026-01-01T00:00:00Z' },
  ],
  createdAt: '2026-02-01T00:00:00Z',
  updatedAt: '2026-02-01T00:00:00Z',
};

describe('EventCard', () => {
  it('renders event title and basic information', () => {
    renderWithRouter(<EventCard event={mockEvent} />);

    expect(screen.getByText('Beach Cleanup Event')).toBeInTheDocument();
    expect(screen.getByText(/123 Beach Road, Coastal City/)).toBeInTheDocument();
    expect(screen.getByText(/Organized by John Organizer/)).toBeInTheDocument();
  });

  it('displays event image when imageUrl is provided', () => {
    renderWithRouter(<EventCard event={mockEvent} />);

    const image = screen.getByAltText('Beach Cleanup Event');
    expect(image).toBeInTheDocument();
    expect(image).toHaveAttribute('src', 'https://example.com/beach.jpg');
  });

  it('displays placeholder icon when no imageUrl', () => {
    const eventWithoutImage = { ...mockEvent, imageUrl: undefined };
    renderWithRouter(<EventCard event={eventWithoutImage} />);

    // Should render SVG calendar icon
    const svg = screen.getByRole('link').querySelector('svg');
    expect(svg).toBeInTheDocument();
  });

  it('displays capacity information correctly', () => {
    renderWithRouter(<EventCard event={mockEvent} />);

    expect(screen.getByText('25 / 50')).toBeInTheDocument();
  });

  it('shows "Full" badge when event is at capacity', () => {
    const fullEvent = { ...mockEvent, registrationCount: 50 };
    renderWithRouter(<EventCard event={fullEvent} />);

    expect(screen.getByText('Full')).toBeInTheDocument();
  });

  it('shows "Cancelled" badge when event is cancelled', () => {
    const cancelledEvent = { ...mockEvent, status: EventStatus.Cancelled };
    renderWithRouter(<EventCard event={cancelledEvent} />);

    expect(screen.getByText('Cancelled')).toBeInTheDocument();
  });

  it('displays duration formatted correctly (hours and minutes)', () => {
    renderWithRouter(<EventCard event={mockEvent} />);

    expect(screen.getByText('3h')).toBeInTheDocument();
  });

  it('displays duration formatted correctly (minutes only)', () => {
    const shortEvent = { ...mockEvent, durationMinutes: 45 };
    renderWithRouter(<EventCard event={shortEvent} />);

    expect(screen.getByText('45m')).toBeInTheDocument();
  });

  it('displays required skills as badges', () => {
    renderWithRouter(<EventCard event={mockEvent} />);

    expect(screen.getByText('First Aid')).toBeInTheDocument();
    expect(screen.getByText('Driving')).toBeInTheDocument();
  });

  it('shows "+X more" indicator when more than 3 skills', () => {
    const manySkills = Array.from({ length: 5 }, (_, i) => ({
      id: i + 1,
      name: `Skill ${i + 1}`,
      description: 'Test Category',
      createdAt: '2026-01-01T00:00:00Z',
    }));
    const eventWithManySkills = { ...mockEvent, requiredSkills: manySkills };

    renderWithRouter(<EventCard event={eventWithManySkills} />);

    // Should show first 3 skills
    expect(screen.getByText('Skill 1')).toBeInTheDocument();
    expect(screen.getByText('Skill 3')).toBeInTheDocument();

    // Should show +2 more badge
    expect(screen.getByText('+2 more')).toBeInTheDocument();

    // Should NOT show skill 4 and 5
    expect(screen.queryByText('Skill 4')).not.toBeInTheDocument();
    expect(screen.queryByText('Skill 5')).not.toBeInTheDocument();
  });

  it('links to event detail page', () => {
    renderWithRouter(<EventCard event={mockEvent} />);

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', '/events/1');
  });

  it('shows green capacity indicator when under 80%', () => {
    const lowCapacityEvent = { ...mockEvent, registrationCount: 10, capacity: 50 }; // 20%
    const { container } = renderWithRouter(<EventCard event={lowCapacityEvent} />);

    const progressBar = container.querySelector('.bg-green-500');
    expect(progressBar).toBeInTheDocument();
  });

  it('shows yellow capacity indicator when 80-99% full', () => {
    const almostFullEvent = { ...mockEvent, registrationCount: 45, capacity: 50 }; // 90%
    const { container } = renderWithRouter(<EventCard event={almostFullEvent} />);

    const progressBar = container.querySelector('.bg-yellow-500');
    expect(progressBar).toBeInTheDocument();
  });

  it('shows orange capacity indicator when 100% full', () => {
    const fullEvent = { ...mockEvent, registrationCount: 50, capacity: 50 }; // 100%
    const { container } = renderWithRouter(<EventCard event={fullEvent} />);

    const progressBar = container.querySelector('.bg-orange-500');
    expect(progressBar).toBeInTheDocument();
  });

  it('shows "Almost Full" badge when event is >80% full but not full', () => {
    const almostFullEvent = { ...mockEvent, registrationCount: 45, capacity: 50 }; // 90%
    renderWithRouter(<EventCard event={almostFullEvent} />);

    expect(screen.getByText('Almost Full')).toBeInTheDocument();
  });

  it('does not show "Almost Full" badge when event is exactly at 80%', () => {
    const eightyPercentEvent = { ...mockEvent, registrationCount: 40, capacity: 50 }; // 80%
    renderWithRouter(<EventCard event={eightyPercentEvent} />);

    expect(screen.queryByText('Almost Full')).not.toBeInTheDocument();
  });

  it('does not show "Almost Full" badge when event is full', () => {
    const fullEvent = { ...mockEvent, registrationCount: 50, capacity: 50 }; // 100%
    renderWithRouter(<EventCard event={fullEvent} />);

    expect(screen.queryByText('Almost Full')).not.toBeInTheDocument();
    expect(screen.getByText('Full')).toBeInTheDocument();
  });

  it('does not show "Almost Full" badge when event is cancelled', () => {
    const cancelledAlmostFullEvent = {
      ...mockEvent,
      registrationCount: 45,
      capacity: 50,
      status: EventStatus.Cancelled,
    };
    renderWithRouter(<EventCard event={cancelledAlmostFullEvent} />);

    expect(screen.queryByText('Almost Full')).not.toBeInTheDocument();
    expect(screen.getByText('Cancelled')).toBeInTheDocument();
  });
});
