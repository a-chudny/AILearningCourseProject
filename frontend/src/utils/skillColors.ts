/**
 * Skill category color mappings
 * Maps skill categories (from description field) to Tailwind color classes
 */

export interface SkillColor {
  bg: string;
  text: string;
  border: string;
  hoverBg: string;
  hoverBorder: string;
}

const categoryColorMap: Record<string, SkillColor> = {
  'Medical & Healthcare': {
    bg: 'bg-red-50',
    text: 'text-red-700',
    border: 'border-red-200',
    hoverBg: 'hover:bg-red-100',
    hoverBorder: 'hover:border-red-300',
  },
  'Organization & Management': {
    bg: 'bg-purple-50',
    text: 'text-purple-700',
    border: 'border-purple-200',
    hoverBg: 'hover:bg-purple-100',
    hoverBorder: 'hover:border-purple-300',
  },
  Communication: {
    bg: 'bg-blue-50',
    text: 'text-blue-700',
    border: 'border-blue-200',
    hoverBg: 'hover:bg-blue-100',
    hoverBorder: 'hover:border-blue-300',
  },
  Education: {
    bg: 'bg-indigo-50',
    text: 'text-indigo-700',
    border: 'border-indigo-200',
    hoverBg: 'hover:bg-indigo-100',
    hoverBorder: 'hover:border-indigo-300',
  },
  'Food Service': {
    bg: 'bg-orange-50',
    text: 'text-orange-700',
    border: 'border-orange-200',
    hoverBg: 'hover:bg-orange-100',
    hoverBorder: 'hover:border-orange-300',
  },
  'Manual Labor': {
    bg: 'bg-amber-50',
    text: 'text-amber-700',
    border: 'border-amber-200',
    hoverBg: 'hover:bg-amber-100',
    hoverBorder: 'hover:border-amber-300',
  },
  Environmental: {
    bg: 'bg-green-50',
    text: 'text-green-700',
    border: 'border-green-200',
    hoverBg: 'hover:bg-green-100',
    hoverBorder: 'hover:border-green-300',
  },
  'Media & Arts': {
    bg: 'bg-pink-50',
    text: 'text-pink-700',
    border: 'border-pink-200',
    hoverBg: 'hover:bg-pink-100',
    hoverBorder: 'hover:border-pink-300',
  },
  'Marketing & Communications': {
    bg: 'bg-cyan-50',
    text: 'text-cyan-700',
    border: 'border-cyan-200',
    hoverBg: 'hover:bg-cyan-100',
    hoverBorder: 'hover:border-cyan-300',
  },
  'Language Services': {
    bg: 'bg-violet-50',
    text: 'text-violet-700',
    border: 'border-violet-200',
    hoverBg: 'hover:bg-violet-100',
    hoverBorder: 'hover:border-violet-300',
  },
  Transportation: {
    bg: 'bg-slate-50',
    text: 'text-slate-700',
    border: 'border-slate-200',
    hoverBg: 'hover:bg-slate-100',
    hoverBorder: 'hover:border-slate-300',
  },
  'Care Services': {
    bg: 'bg-rose-50',
    text: 'text-rose-700',
    border: 'border-rose-200',
    hoverBg: 'hover:bg-rose-100',
    hoverBorder: 'hover:border-rose-300',
  },
};

// Default color for unmapped categories
const defaultColor: SkillColor = {
  bg: 'bg-gray-50',
  text: 'text-gray-700',
  border: 'border-gray-200',
  hoverBg: 'hover:bg-gray-100',
  hoverBorder: 'hover:border-gray-300',
};

/**
 * Get color classes for a skill based on its category (description field)
 */
export function getSkillColor(category: string): SkillColor {
  return categoryColorMap[category] || defaultColor;
}

/**
 * Get all unique categories from the color map
 */
export function getAllCategories(): string[] {
  return Object.keys(categoryColorMap);
}
