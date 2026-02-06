import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import CreateEventPage from '@/pages/user/CreateEventPage';

const mockNavigate = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

vi.mock('@/services/skillService', () => ({
  getSkills: vi
    .fn()
    .mockResolvedValue([
      { id: 1, name: 'First Aid', description: 'Medical', createdAt: '2026-01-01' },
    ]),
}));

function renderCreateEventPage() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <CreateEventPage />
      </BrowserRouter>
    </QueryClientProvider>
  );
}

describe('CreateEventPage', () => {
  it('renders page title and description', () => {
    renderCreateEventPage();

    expect(screen.getByText('Create New Event')).toBeInTheDocument();
    expect(screen.getByText(/Fill out the form below/i)).toBeInTheDocument();
  });

  it('renders event form', async () => {
    renderCreateEventPage();

    expect(screen.getByLabelText(/Event Title/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Description/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Create Event/i })).toBeInTheDocument();
  });

  it('renders cancel button', () => {
    renderCreateEventPage();

    expect(screen.getByRole('button', { name: /Cancel/i })).toBeInTheDocument();
  });

  it('has white card layout', () => {
    const { container } = renderCreateEventPage();

    const card = container.querySelector('.bg-white.rounded-lg.shadow-md');
    expect(card).toBeInTheDocument();
  });
});
