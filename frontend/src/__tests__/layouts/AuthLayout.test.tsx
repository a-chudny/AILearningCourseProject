import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, it, expect } from 'vitest';
import AuthLayout from '@/layouts/AuthLayout';

describe('AuthLayout', () => {
  it('renders children correctly', () => {
    render(
      <BrowserRouter>
        <AuthLayout>
          <div>Test Content</div>
        </AuthLayout>
      </BrowserRouter>
    );

    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('displays the portal title', () => {
    render(
      <BrowserRouter>
        <AuthLayout>
          <div>Content</div>
        </AuthLayout>
      </BrowserRouter>
    );

    expect(screen.getByText('Volunteer Event Portal')).toBeInTheDocument();
  });

  it('displays the logo with proper accessibility', () => {
    render(
      <BrowserRouter>
        <AuthLayout>
          <div>Content</div>
        </AuthLayout>
      </BrowserRouter>
    );

    const logo = screen.getByLabelText('Volunteer Event Portal Logo');
    expect(logo).toBeInTheDocument();
  });

  it('displays back to home link', () => {
    render(
      <BrowserRouter>
        <AuthLayout>
          <div>Content</div>
        </AuthLayout>
      </BrowserRouter>
    );

    const backLink = screen.getByRole('link', { name: /back to home/i });
    expect(backLink).toBeInTheDocument();
    expect(backLink).toHaveAttribute('href', '/');
  });

  it('has proper structure with centered layout', () => {
    const { container } = render(
      <BrowserRouter>
        <AuthLayout>
          <div>Content</div>
        </AuthLayout>
      </BrowserRouter>
    );

    // Check that the main wrapper has the centering classes
    const wrapper = container.querySelector('.flex.min-h-screen');
    expect(wrapper).toBeInTheDocument();
    expect(wrapper).toHaveClass('items-center', 'bg-gray-50', 'justify-center');
  });

  it('has a white card container for children', () => {
    const { container } = render(
      <BrowserRouter>
        <AuthLayout>
          <div>Content</div>
        </AuthLayout>
      </BrowserRouter>
    );

    // Check that the card wrapper exists with proper styling
    const card = container.querySelector('.bg-white.shadow-md.rounded-lg');
    expect(card).toBeInTheDocument();
  });
});
