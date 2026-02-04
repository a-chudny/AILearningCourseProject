import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Pagination } from '@/components/events/Pagination';

describe('Pagination', () => {
  it('renders nothing when totalPages is 1', () => {
    const { container } = render(
      <Pagination
        currentPage={1}
        totalPages={1}
        onPageChange={vi.fn()}
        hasPreviousPage={false}
        hasNextPage={false}
      />
    );

    expect(container.firstChild).toBeNull();
  });

  it('renders all page numbers when total pages is small', () => {
    render(
      <Pagination
        currentPage={2}
        totalPages={5}
        onPageChange={vi.fn()}
        hasPreviousPage={true}
        hasNextPage={true}
      />
    );

    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
    expect(screen.getByText('4')).toBeInTheDocument();
    expect(screen.getByText('5')).toBeInTheDocument();
  });

  it('renders ellipsis for large page count', () => {
    render(
      <Pagination
        currentPage={5}
        totalPages={20}
        onPageChange={vi.fn()}
        hasPreviousPage={true}
        hasNextPage={true}
      />
    );

    // Should show: 1 ... 4 5 6 ... 20
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('20')).toBeInTheDocument();
    expect(screen.getAllByText('...')).toHaveLength(2);
  });

  it('disables Previous button when on first page', () => {
    render(
      <Pagination
        currentPage={1}
        totalPages={5}
        onPageChange={vi.fn()}
        hasPreviousPage={false}
        hasNextPage={true}
      />
    );

    const prevButton = screen.getByLabelText('Go to previous page');
    expect(prevButton).toBeDisabled();
  });

  it('disables Next button when on last page', () => {
    render(
      <Pagination
        currentPage={5}
        totalPages={5}
        onPageChange={vi.fn()}
        hasPreviousPage={true}
        hasNextPage={false}
      />
    );

    const nextButton = screen.getByLabelText('Go to next page');
    expect(nextButton).toBeDisabled();
  });

  it('calls onPageChange with correct page number when page button is clicked', async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();

    render(
      <Pagination
        currentPage={2}
        totalPages={5}
        onPageChange={onPageChange}
        hasPreviousPage={true}
        hasNextPage={true}
      />
    );

    await user.click(screen.getByText('3'));
    expect(onPageChange).toHaveBeenCalledWith(3);
  });

  it('calls onPageChange when Previous button is clicked', async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();

    render(
      <Pagination
        currentPage={3}
        totalPages={5}
        onPageChange={onPageChange}
        hasPreviousPage={true}
        hasNextPage={true}
      />
    );

    await user.click(screen.getByLabelText('Go to previous page'));
    expect(onPageChange).toHaveBeenCalledWith(2);
  });

  it('calls onPageChange when Next button is clicked', async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();

    render(
      <Pagination
        currentPage={2}
        totalPages={5}
        onPageChange={onPageChange}
        hasPreviousPage={true}
        hasNextPage={true}
      />
    );

    await user.click(screen.getByLabelText('Go to next page'));
    expect(onPageChange).toHaveBeenCalledWith(3);
  });

  it('highlights current page', () => {
    render(
      <Pagination
        currentPage={3}
        totalPages={5}
        onPageChange={vi.fn()}
        hasPreviousPage={true}
        hasNextPage={true}
      />
    );

    const currentPageButton = screen.getByLabelText('Go to page 3');
    expect(currentPageButton).toHaveAttribute('aria-current', 'page');
    expect(currentPageButton).toBeDisabled();
  });
});
