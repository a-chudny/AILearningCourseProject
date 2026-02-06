import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { MainLayout } from '@/layouts/MainLayout';
import { AuthProvider } from '@/context/AuthContext';

function renderMainLayout(children: React.ReactNode) {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <MainLayout>{children}</MainLayout>
      </AuthProvider>
    </BrowserRouter>
  );
}

describe('MainLayout', () => {
  it('renders children content', () => {
    renderMainLayout(<div>Test Content</div>);

    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('renders header component', () => {
    renderMainLayout(<div>Content</div>);

    expect(screen.getByLabelText('Volunteer Event Portal Logo')).toBeInTheDocument();
    expect(screen.getByText('Volunteer Portal')).toBeInTheDocument();
  });

  it('renders footer component', () => {
    renderMainLayout(<div>Content</div>);

    const currentYear = new Date().getFullYear();
    expect(
      screen.getByText(new RegExp(`${currentYear} Volunteer Event Portal`))
    ).toBeInTheDocument();
  });

  it('applies correct layout structure', () => {
    const { container } = renderMainLayout(<div>Test</div>);

    const mainElement = container.querySelector('main');
    expect(mainElement).toBeInTheDocument();
    expect(mainElement).toHaveClass('flex-1', 'pt-16');
  });

  it('wraps content with max-width container', () => {
    const { container } = renderMainLayout(<div>Test</div>);

    const mainElement = container.querySelector('main');
    const contentDiv = mainElement?.firstChild;
    expect(contentDiv).toHaveClass('mx-auto', 'max-w-full');
  });
});
