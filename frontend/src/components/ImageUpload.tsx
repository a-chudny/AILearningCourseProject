import { useState, useRef, type DragEvent, type ChangeEvent } from 'react';

export interface ImageUploadProps {
  /**
   * Current image URL (if any)
   */
  imageUrl?: string;
  /**
   * Callback when image is selected
   */
  onImageSelect: (file: File) => void;
  /**
   * Callback when image is removed
   */
  onImageRemove: () => void;
  /**
   * Whether upload is in progress
   */
  isUploading?: boolean;
  /**
   * Whether component is disabled
   */
  disabled?: boolean;
  /**
   * Error message to display
   */
  error?: string;
}

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png'];

/**
 * Image upload component with drag-and-drop support
 */
export function ImageUpload({
  imageUrl,
  onImageSelect,
  onImageRemove,
  isUploading = false,
  disabled = false,
  error,
}: ImageUploadProps) {
  const [isDragging, setIsDragging] = useState(false);
  const [validationError, setValidationError] = useState<string>();
  const fileInputRef = useRef<HTMLInputElement>(null);

  /**
   * Validate file
   */
  const validateFile = (file: File): string | undefined => {
    if (!ALLOWED_TYPES.includes(file.type)) {
      return 'Only JPG and PNG images are allowed';
    }
    if (file.size > MAX_FILE_SIZE) {
      return 'Image size must be less than 5MB';
    }
    return undefined;
  };

  /**
   * Handle file selection
   */
  const handleFileSelect = (file: File) => {
    const error = validateFile(file);
    if (error) {
      setValidationError(error);
      return;
    }

    setValidationError(undefined);
    onImageSelect(file);
  };

  /**
   * Handle file input change
   */
  const handleFileInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  /**
   * Handle drag enter
   */
  const handleDragEnter = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    if (!disabled && !isUploading) {
      setIsDragging(true);
    }
  };

  /**
   * Handle drag leave
   */
  const handleDragLeave = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  };

  /**
   * Handle drag over
   */
  const handleDragOver = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
  };

  /**
   * Handle drop
   */
  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);

    if (disabled || isUploading) return;

    const file = e.dataTransfer.files[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  /**
   * Handle click on drop zone
   */
  const handleClick = () => {
    if (!disabled && !isUploading) {
      fileInputRef.current?.click();
    }
  };

  /**
   * Handle remove image
   */
  const handleRemove = () => {
    setValidationError(undefined);
    onImageRemove();
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const displayError = error || validationError;

  return (
    <div className="w-full">
      {/* Hidden file input */}
      <input
        ref={fileInputRef}
        type="file"
        accept="image/jpeg,image/png"
        onChange={handleFileInputChange}
        className="hidden"
        disabled={disabled || isUploading}
      />

      {/* Image preview or drop zone */}
      {imageUrl ? (
        <div className="relative">
          <img
            src={imageUrl}
            alt="Event banner"
            className="w-full h-48 object-cover rounded-lg border-2 border-gray-300"
          />
          {!disabled && !isUploading && (
            <button
              type="button"
              onClick={handleRemove}
              className="absolute top-2 right-2 bg-red-600 text-white px-3 py-1 rounded-md text-sm font-medium hover:bg-red-700 shadow-lg"
            >
              Remove
            </button>
          )}
          {isUploading && (
            <div className="absolute inset-0 bg-black bg-opacity-50 rounded-lg flex items-center justify-center">
              <div className="flex flex-col items-center gap-2">
                <div className="w-8 h-8 border-4 border-white border-t-transparent rounded-full animate-spin"></div>
                <span className="text-white text-sm font-medium">Uploading...</span>
              </div>
            </div>
          )}
        </div>
      ) : (
        <div
          onDragEnter={handleDragEnter}
          onDragLeave={handleDragLeave}
          onDragOver={handleDragOver}
          onDrop={handleDrop}
          onClick={handleClick}
          className={`
            w-full h-48 border-2 border-dashed rounded-lg
            flex flex-col items-center justify-center gap-3
            transition-colors cursor-pointer
            ${isDragging ? 'border-blue-500 bg-blue-50' : 'border-gray-300 bg-gray-50'}
            ${disabled || isUploading ? 'opacity-50 cursor-not-allowed' : 'hover:border-blue-400 hover:bg-blue-50'}
          `}
        >
          {isUploading ? (
            <>
              <div className="w-12 h-12 border-4 border-blue-500 border-t-transparent rounded-full animate-spin"></div>
              <p className="text-sm text-gray-600 font-medium">Uploading image...</p>
            </>
          ) : (
            <>
              <svg
                className="w-12 h-12 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
                />
              </svg>
              <div className="text-center">
                <p className="text-sm text-gray-700 font-medium">
                  Drag and drop an image here, or click to browse
                </p>
                <p className="text-xs text-gray-500 mt-1">JPG or PNG, max 5MB</p>
              </div>
            </>
          )}
        </div>
      )}

      {/* Error message */}
      {displayError && <p className="mt-2 text-sm text-red-600">{displayError}</p>}
    </div>
  );
}
