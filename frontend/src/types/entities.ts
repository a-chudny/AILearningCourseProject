import type { UserRole, RegistrationStatus, EventStatus } from './enums';

/**
 * Skill entity - predefined volunteer skills
 */
export interface Skill {
  id: number;
  name: string;
  description: string; // Category
  createdAt: string; // ISO 8601 date string
}

/**
 * User entity - platform user with authentication
 */
export interface User {
  id: number;
  email: string;
  name: string;
  role: UserRole;
  phoneNumber?: string;
  createdAt: string; // ISO 8601 date string
  updatedAt: string; // ISO 8601 date string
  skills?: Skill[];
}

/**
 * Event entity - volunteer event with details
 */
export interface Event {
  id: number;
  title: string;
  description: string;
  location: string;
  startTime: string; // ISO 8601 date-time string
  durationMinutes: number;
  capacity: number;
  imageUrl?: string;
  registrationDeadline?: string; // ISO 8601 date-time string
  status: EventStatus;
  organizerId: number;
  organizer?: User;
  createdAt: string; // ISO 8601 date string
  updatedAt: string; // ISO 8601 date string
  requiredSkills?: Skill[];
  registrationCount?: number;
  availableSpots?: number;
  isFull?: boolean;
}

/**
 * Registration entity - user registration for an event
 */
export interface Registration {
  id: number;
  eventId: number;
  userId: number;
  status: RegistrationStatus;
  registeredAt: string; // ISO 8601 date-time string
  notes?: string;
  event?: Event;
  user?: User;
}

/**
 * UserSkill join entity - many-to-many relationship
 */
export interface UserSkill {
  userId: number;
  skillId: number;
  user?: User;
  skill?: Skill;
}

/**
 * EventSkill join entity - many-to-many relationship
 */
export interface EventSkill {
  eventId: number;
  skillId: number;
  event?: Event;
  skill?: Skill;
}
