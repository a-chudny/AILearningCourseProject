import type { Skill } from '@/types'

/**
 * Mock skills data for development
 * This will be replaced with API call when backend skill endpoints are ready
 */
export const mockSkills: Skill[] = [
  { id: 1, name: 'First Aid', description: 'Medical & Healthcare', createdAt: '2026-01-01T00:00:00Z' },
  { id: 2, name: 'CPR Certified', description: 'Medical & Healthcare', createdAt: '2026-01-01T00:00:00Z' },
  { id: 3, name: 'Event Planning', description: 'Organization & Management', createdAt: '2026-01-01T00:00:00Z' },
  { id: 4, name: 'Public Speaking', description: 'Communication', createdAt: '2026-01-01T00:00:00Z' },
  { id: 5, name: 'Teaching', description: 'Education', createdAt: '2026-01-01T00:00:00Z' },
  { id: 6, name: 'Cooking', description: 'Food Service', createdAt: '2026-01-01T00:00:00Z' },
  { id: 7, name: 'Construction', description: 'Manual Labor', createdAt: '2026-01-01T00:00:00Z' },
  { id: 8, name: 'Gardening', description: 'Environmental', createdAt: '2026-01-01T00:00:00Z' },
  { id: 9, name: 'Photography', description: 'Media & Arts', createdAt: '2026-01-01T00:00:00Z' },
  { id: 10, name: 'Social Media', description: 'Marketing & Communications', createdAt: '2026-01-01T00:00:00Z' },
  { id: 11, name: 'Graphic Design', description: 'Media & Arts', createdAt: '2026-01-01T00:00:00Z' },
  { id: 12, name: 'Translation', description: 'Language Services', createdAt: '2026-01-01T00:00:00Z' },
  { id: 13, name: 'Driving', description: 'Transportation', createdAt: '2026-01-01T00:00:00Z' },
  { id: 14, name: 'Fundraising', description: 'Organization & Management', createdAt: '2026-01-01T00:00:00Z' },
  { id: 15, name: 'Child Care', description: 'Care Services', createdAt: '2026-01-01T00:00:00Z' },
]

/**
 * Get all skills (mock implementation)
 * TODO: Replace with actual API call when backend is ready
 */
export async function getSkills(): Promise<Skill[]> {
  // Simulate API delay
  await new Promise(resolve => setTimeout(resolve, 100))
  return mockSkills
}
