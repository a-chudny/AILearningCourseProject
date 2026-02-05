import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ImageUpload } from '@/components/ImageUpload'

describe('ImageUpload', () => {
  const mockOnImageSelect = vi.fn()
  const mockOnImageRemove = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders upload area with instructions', () => {
    render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    expect(screen.getByText(/Drag and drop/i)).toBeInTheDocument()
    expect(screen.getByText(/JPG or PNG, max 5MB/i)).toBeInTheDocument()
  })

  it('displays preview when imageUrl is provided', () => {
    render(
      <ImageUpload
        imageUrl="https://example.com/image.jpg"
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    const image = screen.getByRole('img')
    expect(image).toHaveAttribute('src', 'https://example.com/image.jpg')
  })

  it('shows remove button when image is displayed', () => {
    render(
      <ImageUpload
        imageUrl="https://example.com/image.jpg"
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    const removeButton = screen.getByRole('button', { name: /remove/i })
    expect(removeButton).toBeInTheDocument()
  })

  it('calls onImageRemove when remove button clicked', () => {
    render(
      <ImageUpload
        imageUrl="https://example.com/image.jpg"
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    const removeButton = screen.getByRole('button', { name: /remove/i })
    fireEvent.click(removeButton)

    expect(mockOnImageRemove).toHaveBeenCalledTimes(1)
  })

  it('shows uploading state', () => {
    render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
        isUploading={true}
      />
    )

    expect(screen.getByText(/uploading/i)).toBeInTheDocument()
  })

  it('shows error message when provided', () => {
    render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
        error="Upload failed"
      />
    )

    expect(screen.getByText('Upload failed')).toBeInTheDocument()
  })

  it('disables interactions when disabled', () => {
    const { container } = render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
        disabled={true}
      />
    )

    const dropzone = container.querySelector('.border-dashed')
    expect(dropzone).toHaveClass('cursor-not-allowed')
  })

  it('accepts file input selection', async () => {
    render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    const file = new File(['test'], 'test.png', { type: 'image/png' })
    const input = document.querySelector('input[type="file"]') as HTMLInputElement

    fireEvent.change(input, { target: { files: [file] } })

    expect(mockOnImageSelect).toHaveBeenCalledWith(file)
  })

  it('validates file type and shows error for invalid types', () => {
    render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    const file = new File(['test'], 'test.gif', { type: 'image/gif' })
    const input = document.querySelector('input[type="file"]') as HTMLInputElement

    fireEvent.change(input, { target: { files: [file] } })

    expect(mockOnImageSelect).not.toHaveBeenCalled()
    expect(screen.getByText(/Only JPG and PNG/i)).toBeInTheDocument()
  })

  it('validates file size and shows error for oversized files', () => {
    render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    // Create a file larger than 5MB (simulated)
    const largeContent = new Array(6 * 1024 * 1024).fill('a').join('')
    const file = new File([largeContent], 'large.png', { type: 'image/png' })
    const input = document.querySelector('input[type="file"]') as HTMLInputElement

    fireEvent.change(input, { target: { files: [file] } })

    expect(mockOnImageSelect).not.toHaveBeenCalled()
    expect(screen.getByText(/less than 5MB/i)).toBeInTheDocument()
  })

  it('handles drag and drop', async () => {
    const { container } = render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    const dropzone = container.querySelector('.border-dashed') as HTMLElement
    const file = new File(['test'], 'test.png', { type: 'image/png' })

    fireEvent.dragEnter(dropzone, {
      dataTransfer: { files: [file] }
    })

    // Should show drag active state
    await waitFor(() => {
      expect(dropzone).toHaveClass('border-blue-500')
    })
  })

  it('removes drag state on drag leave', () => {
    const { container } = render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    const dropzone = container.querySelector('.border-dashed') as HTMLElement
    
    fireEvent.dragEnter(dropzone)
    fireEvent.dragLeave(dropzone)

    expect(dropzone).not.toHaveClass('border-blue-500')
  })

  it('processes dropped file', async () => {
    const { container } = render(
      <ImageUpload
        onImageSelect={mockOnImageSelect}
        onImageRemove={mockOnImageRemove}
      />
    )

    const dropzone = container.querySelector('.border-dashed') as HTMLElement
    const file = new File(['test'], 'test.png', { type: 'image/png' })

    // Create a proper DataTransfer-like object
    const dataTransfer = {
      files: [file],
      items: [{ kind: 'file', type: file.type, getAsFile: () => file }],
      types: ['Files'],
    }

    fireEvent.drop(dropzone, { dataTransfer })

    await waitFor(() => {
      expect(mockOnImageSelect).toHaveBeenCalledWith(file)
    })
  })
})
