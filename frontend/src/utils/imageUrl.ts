/**
 * Constructs full URL for backend images
 * Handles relative paths from the backend by prepending the API base URL
 */
export function getImageUrl(imageUrl: string | null | undefined): string | undefined {
  if (!imageUrl) return undefined;

  // If already a full URL (http/https), return as-is
  if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
    return imageUrl;
  }

  // Relative path - prepend backend base URL
  // In development, Vite proxies /api to backend, but static files need direct backend URL
  const backendUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
  return backendUrl + imageUrl;
}
