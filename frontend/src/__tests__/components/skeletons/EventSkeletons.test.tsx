import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import {
  EventCardSkeleton,
  EventDetailsSkeleton,
  RegistrationCardSkeleton,
} from '@/components/skeletons/EventSkeletons';

describe('EventCardSkeleton', () => {
  it('renders with animate-pulse class', () => {
    const { container } = render(<EventCardSkeleton />);

    const skeleton = container.firstChild as HTMLElement;
    expect(skeleton).toHaveClass('animate-pulse');
  });

  it('renders image placeholder', () => {
    const { container } = render(<EventCardSkeleton />);

    // Image placeholder has h-48 height
    const imagePlaceholder = container.querySelector('.h-48');
    expect(imagePlaceholder).toBeInTheDocument();
    expect(imagePlaceholder).toHaveClass('bg-gray-200');
  });

  it('renders title placeholder', () => {
    const { container } = render(<EventCardSkeleton />);

    // Title placeholder has h-6 height
    const titlePlaceholder = container.querySelector('.h-6.bg-gray-200');
    expect(titlePlaceholder).toBeInTheDocument();
  });

  it('renders button placeholder', () => {
    const { container } = render(<EventCardSkeleton />);

    // Button placeholder has h-10 height
    const buttonPlaceholder = container.querySelector('.h-10');
    expect(buttonPlaceholder).toBeInTheDocument();
  });

  it('has proper card structure with border and shadow', () => {
    const { container } = render(<EventCardSkeleton />);

    const card = container.firstChild as HTMLElement;
    expect(card).toHaveClass('border');
    expect(card).toHaveClass('border-gray-200');
    expect(card).toHaveClass('shadow-sm');
    expect(card).toHaveClass('rounded-lg');
  });
});

describe('EventDetailsSkeleton', () => {
  it('renders with animate-pulse class', () => {
    const { container } = render(<EventDetailsSkeleton />);

    const skeleton = container.firstChild as HTMLElement;
    expect(skeleton).toHaveClass('animate-pulse');
  });

  it('renders large image placeholder', () => {
    const { container } = render(<EventDetailsSkeleton />);

    // Image placeholder has h-64 height
    const imagePlaceholder = container.querySelector('.h-64');
    expect(imagePlaceholder).toBeInTheDocument();
    expect(imagePlaceholder).toHaveClass('bg-gray-200');
    expect(imagePlaceholder).toHaveClass('rounded-lg');
  });

  it('renders title placeholder', () => {
    const { container } = render(<EventDetailsSkeleton />);

    // Title has h-8 height
    const titlePlaceholder = container.querySelector('.h-8.bg-gray-200');
    expect(titlePlaceholder).toBeInTheDocument();
  });

  it('renders meta info placeholders', () => {
    const { container } = render(<EventDetailsSkeleton />);

    // Meta placeholders have h-5 height and w-32 width
    const metaPlaceholders = container.querySelectorAll('.h-5.bg-gray-200.w-32');
    expect(metaPlaceholders.length).toBe(3);
  });

  it('renders description placeholders', () => {
    const { container } = render(<EventDetailsSkeleton />);

    // Multiple h-4 lines for description
    const descriptionLines = container.querySelectorAll('.h-4.bg-gray-200');
    expect(descriptionLines.length).toBeGreaterThan(0);
  });

  it('renders skill badge placeholders', () => {
    const { container } = render(<EventDetailsSkeleton />);

    // Skill badges have h-6 and w-20
    const skillPlaceholders = container.querySelectorAll('.h-6.bg-gray-200.w-20');
    expect(skillPlaceholders.length).toBe(3);
  });

  it('renders action button placeholder', () => {
    const { container } = render(<EventDetailsSkeleton />);

    // Button placeholder has h-12 height
    const buttonPlaceholder = container.querySelector('.h-12');
    expect(buttonPlaceholder).toBeInTheDocument();
  });
});

describe('RegistrationCardSkeleton', () => {
  it('renders with animate-pulse class', () => {
    const { container } = render(<RegistrationCardSkeleton />);

    const skeleton = container.firstChild as HTMLElement;
    expect(skeleton).toHaveClass('animate-pulse');
  });

  it('has proper card structure', () => {
    const { container } = render(<RegistrationCardSkeleton />);

    const card = container.firstChild as HTMLElement;
    expect(card).toHaveClass('border');
    expect(card).toHaveClass('border-gray-200');
    expect(card).toHaveClass('rounded-lg');
    expect(card).toHaveClass('shadow-sm');
    expect(card).toHaveClass('bg-white');
    expect(card).toHaveClass('p-6');
  });

  it('renders content placeholders', () => {
    const { container } = render(<RegistrationCardSkeleton />);

    // Content placeholders
    const h6Placeholders = container.querySelectorAll('.h-6.bg-gray-200');
    expect(h6Placeholders.length).toBeGreaterThan(0);
  });

  it('renders status badge placeholder', () => {
    const { container } = render(<RegistrationCardSkeleton />);

    // Status badge has w-24 width
    const statusPlaceholder = container.querySelector('.w-24');
    expect(statusPlaceholder).toBeInTheDocument();
  });

  it('renders action button placeholders', () => {
    const { container } = render(<RegistrationCardSkeleton />);

    // Button placeholders have h-10 height
    const buttonPlaceholders = container.querySelectorAll('.h-10');
    expect(buttonPlaceholders.length).toBeGreaterThan(0);
  });
});
