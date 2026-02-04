/**
 * Generic API response wrapper
 */
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

/**
 * Paginated API response
 */
export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * API error response
 */
export interface ApiError {
  message: string;
  code?: string;
  errors?: Record<string, string[]>; // Validation errors
  statusCode?: number;
}

/**
 * Query parameters for list endpoints
 */
export interface QueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * Event query parameters - matches backend EventQueryParams
 */
export interface EventQueryParams extends QueryParams {
  includePastEvents?: boolean; // Include past events (default: false)
  includeDeleted?: boolean; // Include soft-deleted events (default: false)
  searchTerm?: string; // Search by title/description
  status?: string; // Filter by EventStatus (Active/Cancelled)
  sortBy?: string; // Sort field: StartTime, Title, CreatedAt (default: StartTime)
  sortDirection?: 'asc' | 'desc'; // Sort direction (default: asc)
}

/**
 * User query parameters
 */
export interface UserQueryParams extends QueryParams {
  role?: number;
}
