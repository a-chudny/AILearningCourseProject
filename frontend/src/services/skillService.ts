import { api } from './api';
import type { Skill } from '@/types';

/**
 * DTO for updating user skills
 */
export interface UpdateUserSkillsRequest {
  skillIds: number[];
}

/**
 * Get all available skills
 */
export async function getSkills(): Promise<Skill[]> {
  const response = await api.get<Skill[]>('/skills');
  return response.data;
}

/**
 * Get current user's skills
 * Requires authentication
 */
export async function getUserSkills(): Promise<Skill[]> {
  const response = await api.get<Skill[]>('/skills/me');
  return response.data;
}

/**
 * Update current user's skills (complete replace)
 * Requires authentication
 */
export async function updateUserSkills(skillIds: number[]): Promise<void> {
  await api.put('/skills/me', { skillIds });
}
