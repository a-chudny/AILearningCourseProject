import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Logo } from '@/assets/Logo';

describe('Logo', () => {
  it('renders logo SVG with default props', () => {
    render(<Logo />);

    const logo = screen.getByLabelText('Volunteer Event Portal Logo');
    expect(logo).toBeInTheDocument();
    expect(logo).toHaveAttribute('width', '64');
    expect(logo).toHaveAttribute('height', '64');
  });

  it('renders logo with custom size', () => {
    render(<Logo size={48} />);

    const logo = screen.getByLabelText('Volunteer Event Portal Logo');
    expect(logo).toHaveAttribute('width', '48');
    expect(logo).toHaveAttribute('height', '48');
  });

  it('applies custom className', () => {
    render(<Logo className="text-red-600" />);

    const logo = screen.getByLabelText('Volunteer Event Portal Logo');
    expect(logo).toHaveClass('text-red-600');
  });

  it('has correct SVG structure', () => {
    const { container } = render(<Logo />);

    const svg = container.querySelector('svg');
    expect(svg).toBeInTheDocument();
    expect(svg).toHaveAttribute('viewBox', '0 0 64 64');

    const heartPath = container.querySelector('path[fill="currentColor"]');
    expect(heartPath).toBeInTheDocument();

    const handsPath = container.querySelector('path[stroke="white"]');
    expect(handsPath).toBeInTheDocument();

    const circle = container.querySelector('circle');
    expect(circle).toBeInTheDocument();
  });
});
