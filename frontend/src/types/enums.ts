// Enum definitions using const object pattern for TypeScript 5.9 compatibility

/**
 * User role enum - matches backend UserRole enum values
 */
export const UserRole = {
  Volunteer: 0,
  Organizer: 1,
  Admin: 2,
} as const;

export type UserRole = (typeof UserRole)[keyof typeof UserRole];

/**
 * Registration status enum - matches backend RegistrationStatus enum
 */
export const RegistrationStatus = {
  Confirmed: 'Confirmed',
  Cancelled: 'Cancelled',
} as const;

export type RegistrationStatus = (typeof RegistrationStatus)[keyof typeof RegistrationStatus];

/**
 * Event status enum - matches backend EventStatus enum
 */
export const EventStatus = {
  Active: 'Active',
  Cancelled: 'Cancelled',
} as const;

export type EventStatus = (typeof EventStatus)[keyof typeof EventStatus];

// Helper functions for enum display
export const UserRoleLabels: Record<UserRole, string> = {
  [UserRole.Volunteer]: 'Volunteer',
  [UserRole.Organizer]: 'Organizer',
  [UserRole.Admin]: 'Admin',
};
