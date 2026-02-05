import { render, screen, fireEvent } from '@testing-library/react'
import { describe, it, expect, vi } from 'vitest'
import { SkillSelector } from '@/components/skills/SkillSelector'
import type { Skill } from '@/types'

// Mock skills with different categories
const mockSkills: Skill[] = [
  { id: 1, name: 'React', description: 'Technology' },
  { id: 2, name: 'Node.js', description: 'Technology' },
  { id: 3, name: 'Python', description: 'Technology' },
  { id: 4, name: 'Leadership', description: 'Community' },
  { id: 5, name: 'Communication', description: 'Community' },
  { id: 6, name: 'Teaching', description: 'Education' },
]

describe('SkillSelector', () => {
  it('renders with label when provided', () => {
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[]}
        onChange={() => {}}
        label="Select Skills"
      />
    )

    expect(screen.getByText('Select Skills')).toBeInTheDocument()
  })

  it('groups skills by category', () => {
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[]}
        onChange={() => {}}
      />
    )

    expect(screen.getByText('Technology')).toBeInTheDocument()
    expect(screen.getByText('Community')).toBeInTheDocument()
    expect(screen.getByText('Education')).toBeInTheDocument()
  })

  it('shows skill count in category header', () => {
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[1, 2]}
        onChange={() => {}}
      />
    )

    // Technology has 2 selected out of 3
    expect(screen.getByText('(2/3)')).toBeInTheDocument()
  })

  it('displays all skills within expanded categories', () => {
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[]}
        onChange={() => {}}
      />
    )

    // All categories are expanded by default
    expect(screen.getByText('React')).toBeInTheDocument()
    expect(screen.getByText('Node.js')).toBeInTheDocument()
    expect(screen.getByText('Python')).toBeInTheDocument()
    expect(screen.getByText('Leadership')).toBeInTheDocument()
    expect(screen.getByText('Teaching')).toBeInTheDocument()
  })

  it('calls onChange when skill is toggled', () => {
    const handleChange = vi.fn()
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[]}
        onChange={handleChange}
      />
    )

    // Click on React checkbox
    const reactCheckbox = screen.getByRole('checkbox', { name: /React/i })
    fireEvent.click(reactCheckbox)

    expect(handleChange).toHaveBeenCalledWith([1])
  })

  it('removes skill when unchecking selected skill', () => {
    const handleChange = vi.fn()
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[1, 2]}
        onChange={handleChange}
      />
    )

    // Uncheck React
    const reactCheckbox = screen.getByRole('checkbox', { name: /React/i })
    fireEvent.click(reactCheckbox)

    expect(handleChange).toHaveBeenCalledWith([2])
  })

  it('shows checked state for selected skills', () => {
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[1, 4]}
        onChange={() => {}}
      />
    )

    const reactCheckbox = screen.getByRole('checkbox', { name: /React/i }) as HTMLInputElement
    const leadershipCheckbox = screen.getByRole('checkbox', { name: /Leadership/i }) as HTMLInputElement
    const teachingCheckbox = screen.getByRole('checkbox', { name: /Teaching/i }) as HTMLInputElement

    expect(reactCheckbox.checked).toBe(true)
    expect(leadershipCheckbox.checked).toBe(true)
    expect(teachingCheckbox.checked).toBe(false)
  })

  it('collapses and expands categories on header click', () => {
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[]}
        onChange={() => {}}
      />
    )

    // All skills visible initially
    expect(screen.getByText('React')).toBeInTheDocument()

    // Click Technology header to collapse
    const technologyHeader = screen.getByText('Technology')
    fireEvent.click(technologyHeader)

    // React should now be hidden
    expect(screen.queryByText('React')).not.toBeInTheDocument()

    // Click again to expand
    fireEvent.click(technologyHeader)
    expect(screen.getByText('React')).toBeInTheDocument()
  })

  it('selects all skills in category when Select All is clicked', () => {
    const handleChange = vi.fn()
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[]}
        onChange={handleChange}
      />
    )

    // Click "Select All" for Technology category (third in alphabetical order after Community, Education)
    const selectAllButtons = screen.getAllByText('Select All')
    // Categories are sorted: Community, Education, Technology
    fireEvent.click(selectAllButtons[2]) // Technology is third

    expect(handleChange).toHaveBeenCalledWith([1, 2, 3])
  })

  it('clears all skills in category when Clear is clicked', () => {
    const handleChange = vi.fn()
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[1, 2, 3]} // All Technology skills selected
        onChange={handleChange}
      />
    )

    // Click "Clear" for Technology category (categories are sorted alphabetically)
    const clearButtons = screen.getAllByText('Clear')
    // Technology is the only fully selected category, so its Clear button will be shown
    const clearButton = clearButtons[0]
    fireEvent.click(clearButton)

    expect(handleChange).toHaveBeenCalledWith([])
  })

  it('shows selected skills summary', () => {
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[1, 4]}
        onChange={() => {}}
      />
    )

    expect(screen.getByText('Selected Skills (2)')).toBeInTheDocument()
  })

  it('allows removing skills from summary', () => {
    const handleChange = vi.fn()
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[1, 4]}
        onChange={handleChange}
      />
    )

    // Find and click remove button for React in summary
    const removeButtons = screen.getAllByRole('button', { name: /Remove/i })
    fireEvent.click(removeButtons[0])

    // Should remove skill id 1 (React)
    expect(handleChange).toHaveBeenCalledWith([4])
  })

  it('does not show summary when no skills selected', () => {
    render(
      <SkillSelector
        allSkills={mockSkills}
        selectedSkillIds={[]}
        onChange={() => {}}
      />
    )

    expect(screen.queryByText(/Selected Skills/)).not.toBeInTheDocument()
  })
})
