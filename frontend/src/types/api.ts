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
 * Event query parameters
 */
export interface EventQueryParams extends QueryParams {
  skillIds?: number[];
  matchMySkills?: boolean;
  includeAll?: boolean; // Include past events (admin only)
  status?: string; // Filter by EventStatus
}

/**
 * User query parameters
 */
export interface UserQueryParams extends QueryParams {
  role?: number;
}
