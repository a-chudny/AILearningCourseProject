import { useState, useMemo } from 'react'
import type { Skill } from '@/types'

interface SkillSelectorProps {
  /** All available skills */
  allSkills: Skill[]
  /** Currently selected skill IDs */
  selectedSkillIds: number[]
  /** Callback when selection changes */
  onChange: (skillIds: number[]) => void
  /** Optional label */
  label?: string
}

/**
 * Multi-select skill selector with category grouping and accordion UI
 * Skills are grouped by category with expand/collapse functionality
 */
export function SkillSelector({ allSkills, selectedSkillIds, onChange, label }: SkillSelectorProps) {
  // Group skills by category
  const skillsByCategory = useMemo(() => {
    const grouped = new Map<string, Skill[]>()
    
    allSkills.forEach((skill) => {
      const category = skill.category
      if (!grouped.has(category)) {
        grouped.set(category, [])
      }
      grouped.get(category)!.push(skill)
    })
    
    // Sort categories alphabetically
    return Array.from(grouped.entries()).sort((a, b) => a[0].localeCompare(b[0]))
  }, [allSkills])

  // Track which categories are expanded (all expanded by default)
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(
    new Set(skillsByCategory.map(([category]) => category))
  )

  const toggleCategory = (category: string) => {
    setExpandedCategories((prev) => {
      const next = new Set(prev)
      if (next.has(category)) {
        next.delete(category)
      } else {
        next.add(category)
      }
      return next
    })
  }

  const toggleSkill = (skillId: number) => {
    const newSelection = selectedSkillIds.includes(skillId)
      ? selectedSkillIds.filter((id) => id !== skillId)
      : [...selectedSkillIds, skillId]
    onChange(newSelection)
  }

  const selectAllInCategory = (categorySkills: Skill[]) => {
    const categorySkillIds = categorySkills.map((s) => s.id)
    const newSelection = Array.from(new Set([...selectedSkillIds, ...categorySkillIds]))
    onChange(newSelection)
  }

  const deselectAllInCategory = (categorySkills: Skill[]) => {
    const categorySkillIds = new Set(categorySkills.map((s) => s.id))
    const newSelection = selectedSkillIds.filter((id) => !categorySkillIds.has(id))
    onChange(newSelection)
  }

  const isCategoryFullySelected = (categorySkills: Skill[]) => {
    return categorySkills.every((skill) => selectedSkillIds.includes(skill.id))
  }

  const isCategoryPartiallySelected = (categorySkills: Skill[]) => {
    const selected = categorySkills.filter((skill) => selectedSkillIds.includes(skill.id))
    return selected.length > 0 && selected.length < categorySkills.length
  }

  return (
    <div className="space-y-2">
      {label && (
        <label className="block text-sm font-medium text-gray-700 mb-3">{label}</label>
      )}

      <div className="border border-gray-300 rounded-lg divide-y divide-gray-200">
        {skillsByCategory.map(([category, skills]) => {
          const isExpanded = expandedCategories.has(category)
          const isFullySelected = isCategoryFullySelected(skills)
          const isPartiallySelected = isCategoryPartiallySelected(skills)

          return (
            <div key={category} className="bg-white">
              {/* Category Header */}
              <div className="flex items-center justify-between p-4 hover:bg-gray-50 transition-colors">
                <button
                  type="button"
                  onClick={() => toggleCategory(category)}
                  className="flex items-center gap-2 flex-1 text-left"
                  aria-expanded={isExpanded}
                >
                  <svg
                    className={`h-5 w-5 text-gray-500 transition-transform ${
                      isExpanded ? 'rotate-90' : ''
                    }`}
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                  </svg>
                  <span className="font-medium text-gray-900">{category}</span>
                  <span className="text-sm text-gray-500">
                    ({skills.filter((s) => selectedSkillIds.includes(s.id)).length}/{skills.length})
                  </span>
                </button>

                {/* Select/Deselect All */}
                <div className="flex gap-2">
                  {!isFullySelected && (
                    <button
                      type="button"
                      onClick={() => selectAllInCategory(skills)}
                      className="text-xs text-blue-600 hover:text-blue-700 font-medium"
                    >
                      Select All
                    </button>
                  )}
                  {(isFullySelected || isPartiallySelected) && (
                    <button
                      type="button"
                      onClick={() => deselectAllInCategory(skills)}
                      className="text-xs text-gray-600 hover:text-gray-700 font-medium"
                    >
                      Clear
                    </button>
                  )}
                </div>
              </div>

              {/* Skills List */}
              {isExpanded && (
                <div className="px-4 pb-4 pt-2 space-y-2 bg-gray-50">
                  {skills.map((skill) => (
                    <label
                      key={skill.id}
                      className="flex items-center gap-3 p-2 rounded hover:bg-gray-100 cursor-pointer transition-colors"
                    >
                      <input
                        type="checkbox"
                        checked={selectedSkillIds.includes(skill.id)}
                        onChange={() => toggleSkill(skill.id)}
                        className="h-4 w-4 text-blue-600 focus:ring-0 focus:ring-offset-0 border-gray-300 rounded cursor-pointer"
                      />
                      <span className="text-sm text-gray-900">{skill.name}</span>
                    </label>
                  ))}
                </div>
              )}
            </div>
          )
        })}
      </div>

      {/* Selected Skills Summary */}
      {selectedSkillIds.length > 0 && (
        <div className="mt-4">
          <p className="text-sm font-medium text-gray-700 mb-2">
            Selected Skills ({selectedSkillIds.length})
          </p>
          <div className="flex flex-wrap gap-2">
            {selectedSkillIds.map((skillId) => {
              const skill = allSkills.find((s) => s.id === skillId)
              if (!skill) return null
              return (
                <span
                  key={skill.id}
                  className="inline-flex items-center gap-1 px-3 py-1 bg-blue-100 text-blue-800 text-sm rounded-full"
                >
                  {skill.name}
                  <button
                    type="button"
                    onClick={() => toggleSkill(skill.id)}
                    className="hover:text-blue-900 focus:outline-none"
                    aria-label={`Remove ${skill.name}`}
                  >
                    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M6 18L18 6M6 6l12 12"
                      />
                    </svg>
                  </button>
                </span>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}
