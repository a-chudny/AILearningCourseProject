// Common types used throughout the application

// Base entity interface with common fields
export interface BaseEntity {
  id: number
  createdAt: string
  updatedAt?: string
}

// User roles - using const object pattern for TypeScript 5.9+ compatibility
export const UserRole = {
  Volunteer: 'Volunteer',
  Organizer: 'Organizer',
  Admin: 'Admin',
} as const

export type UserRole = (typeof UserRole)[keyof typeof UserRole]

// User entity
export interface User extends BaseEntity {
  email: string
  name: string
  role: UserRole
}

// Event status - using const object pattern
export const EventStatus = {
  Draft: 'Draft',
  Published: 'Published',
  Cancelled: 'Cancelled',
  Completed: 'Completed',
} as const

export type EventStatus = (typeof EventStatus)[keyof typeof EventStatus]

// Event entity
export interface Event extends BaseEntity {
  title: string
  description: string
  date: string
  location: string
  capacity: number
  organizerId: number
  organizer?: User
  status: EventStatus
  imageUrl?: string
  registrationCount?: number
}

// Registration status - using const object pattern
export const RegistrationStatus = {
  Pending: 'Pending',
  Confirmed: 'Confirmed',
  Cancelled: 'Cancelled',
  Attended: 'Attended',
  NoShow: 'NoShow',
} as const

export type RegistrationStatus = (typeof RegistrationStatus)[keyof typeof RegistrationStatus]

// Registration entity
export interface Registration extends BaseEntity {
  eventId: number
  userId: number
  status: RegistrationStatus
  registeredAt: string
  event?: Event
  user?: User
}

// Organization entity
export interface Organization extends BaseEntity {
  name: string
  description: string
  contactEmail: string
}

// API Response types
export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

// Auth types
export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  user: User
  expiresAt: string
}

export interface RegisterRequest {
  email: string
  password: string
  name: string
  role?: UserRole
}
