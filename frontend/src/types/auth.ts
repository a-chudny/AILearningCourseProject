import type { User } from './entities';
import type { UserRole } from './enums';

/**
 * Login request payload
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Registration request payload
 */
export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
  phoneNumber?: string;
}

/**
 * Authentication response with token and user info
 */
export interface AuthResponse {
  token: string;
  user: User;
  expiresAt: string; // ISO 8601 date-time string
}

/**
 * JWT token payload (decoded)
 */
export interface TokenPayload {
  userId: number;
  email: string;
  role: UserRole;
  exp: number; // Expiration timestamp
  iat: number; // Issued at timestamp
}

/**
 * Update user skills request
 */
export interface UpdateUserSkillsRequest {
  skillIds: number[];
}

/**
 * Update user role request (admin only)
 */
export interface UpdateUserRoleRequest {
  role: UserRole;
}
