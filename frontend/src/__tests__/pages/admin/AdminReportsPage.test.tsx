import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import AdminReportsPage from '@/pages/admin/AdminReportsPage';
import * as adminService from '@/services/adminService';

// Mock the admin service
vi.mock('@/services/adminService', () => ({
  exportUsers: vi.fn(),
  exportEvents: vi.fn(),
  exportRegistrations: vi.fn(),
  exportSkillsSummary: vi.fn(),
}));

// Mock URL.createObjectURL and revokeObjectURL
global.URL.createObjectURL = vi.fn(() => 'blob:mock-url');
global.URL.revokeObjectURL = vi.fn();

describe('AdminReportsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  const renderPage = () => {
    return render(
      <BrowserRouter>
        <AdminReportsPage />
      </BrowserRouter>
    );
  };

  it('renders page header', () => {
    renderPage();
    expect(screen.getByText('Reports & Exports')).toBeInTheDocument();
    expect(
      screen.getByText('Export platform data to CSV files for analysis and reporting')
    ).toBeInTheDocument();
  });

  it('renders all four export cards', () => {
    renderPage();
    expect(screen.getByText('Users Export')).toBeInTheDocument();
    expect(screen.getByText('Events Export')).toBeInTheDocument();
    expect(screen.getByText('Registrations Export')).toBeInTheDocument();
    expect(screen.getByText('Skills Summary Export')).toBeInTheDocument();
  });

  it('renders export descriptions', () => {
    renderPage();
    expect(
      screen.getByText(/Export all users with their email, role, skills/)
    ).toBeInTheDocument();
    expect(
      screen.getByText(/Export all events including title, date, location/)
    ).toBeInTheDocument();
  });

  it('renders date filters for registrations export', () => {
    renderPage();
    expect(screen.getByLabelText('Start Date')).toBeInTheDocument();
    expect(screen.getByLabelText('End Date')).toBeInTheDocument();
  });

  it('renders information section', () => {
    renderPage();
    expect(screen.getByText('About Exports')).toBeInTheDocument();
    expect(
      screen.getByText(/All exports are in CSV format/)
    ).toBeInTheDocument();
  });

  it('calls exportUsers when users export button is clicked', async () => {
    const mockBlob = new Blob(['test'], { type: 'text/csv' });
    vi.mocked(adminService.exportUsers).mockResolvedValue(mockBlob);

    renderPage();

    const exportButtons = screen.getAllByText('Export CSV');
    await userEvent.click(exportButtons[0]); // First button is Users export

    await waitFor(() => {
      expect(adminService.exportUsers).toHaveBeenCalledTimes(1);
    });
  });

  it('shows loading state during export', async () => {
    vi.mocked(adminService.exportUsers).mockImplementation(
      () => new Promise((resolve) => setTimeout(() => resolve(new Blob()), 100))
    );

    renderPage();

    const exportButtons = screen.getAllByText('Export CSV');
    await userEvent.click(exportButtons[0]);

    expect(screen.getByText('Exporting...')).toBeInTheDocument();
  });

  it('shows success state after successful export', async () => {
    const mockBlob = new Blob(['test'], { type: 'text/csv' });
    vi.mocked(adminService.exportUsers).mockResolvedValue(mockBlob);

    renderPage();

    const exportButtons = screen.getAllByText('Export CSV');
    await userEvent.click(exportButtons[0]);

    await waitFor(() => {
      expect(screen.getByText('Downloaded!')).toBeInTheDocument();
    });
  });

  it('shows error state when export fails', async () => {
    vi.mocked(adminService.exportUsers).mockRejectedValue(new Error('Export failed'));

    renderPage();

    const exportButtons = screen.getAllByText('Export CSV');
    await userEvent.click(exportButtons[0]);

    await waitFor(() => {
      expect(screen.getByText('Failed')).toBeInTheDocument();
    });
  });

  it('updates registration filters when dates are changed', async () => {
    renderPage();

    const startDateInput = screen.getByLabelText('Start Date') as HTMLInputElement;
    const endDateInput = screen.getByLabelText('End Date') as HTMLInputElement;

    await userEvent.type(startDateInput, '2026-01-01');
    await userEvent.type(endDateInput, '2026-12-31');

    expect(startDateInput.value).toBe('2026-01-01');
    expect(endDateInput.value).toBe('2026-12-31');
  });

  it('shows clear filters button when dates are set', async () => {
    renderPage();

    const startDateInput = screen.getByLabelText('Start Date');
    await userEvent.type(startDateInput, '2026-01-01');

    await waitFor(() => {
      expect(screen.getByText('Clear filters')).toBeInTheDocument();
    });
  });

  it('clears filters when clear button is clicked', async () => {
    renderPage();

    const startDateInput = screen.getByLabelText('Start Date') as HTMLInputElement;
    await userEvent.type(startDateInput, '2026-01-01');

    await waitFor(() => {
      expect(screen.getByText('Clear filters')).toBeInTheDocument();
    });

    const clearButton = screen.getByText('Clear filters');
    await userEvent.click(clearButton);

    await waitFor(() => {
      expect(startDateInput.value).toBe('');
    });
  });

  it('passes date filters to exportRegistrations', async () => {
    const mockBlob = new Blob(['test'], { type: 'text/csv' });
    vi.mocked(adminService.exportRegistrations).mockResolvedValue(mockBlob);

    renderPage();

    const startDateInput = screen.getByLabelText('Start Date');
    const endDateInput = screen.getByLabelText('End Date');

    await userEvent.type(startDateInput, '2026-01-01');
    await userEvent.type(endDateInput, '2026-12-31');

    const exportButtons = screen.getAllByText('Export CSV');
    await userEvent.click(exportButtons[2]); // Third button is Registrations export

    await waitFor(() => {
      expect(adminService.exportRegistrations).toHaveBeenCalledWith({
        startDate: '2026-01-01',
        endDate: '2026-12-31',
      });
    });
  });
});
