import { render, screen, fireEvent } from '@testing-library/react'
import { describe, it, expect, vi } from 'vitest'
import { SkillBadge, SkillBadgeList } from '@/components/skills/SkillBadge'
import type { Skill } from '@/types'

// Mock skill data
const mockSkill: Skill = {
  id: 1,
  name: 'React',
  description: 'Technology',
}

const mockSkillCommunity: Skill = {
  id: 2,
  name: 'Leadership',
  description: 'Community & Leadership',
}

const mockSkills: Skill[] = [
  { id: 1, name: 'React', description: 'Technology' },
  { id: 2, name: 'Node.js', description: 'Technology' },
  { id: 3, name: 'Leadership', description: 'Community & Leadership' },
  { id: 4, name: 'Communication', description: 'Community & Leadership' },
  { id: 5, name: 'Teaching', description: 'Education' },
]

describe('SkillBadge', () => {
  it('renders skill name', () => {
    render(<SkillBadge skill={mockSkill} />)
    
    expect(screen.getByText('React')).toBeInTheDocument()
  })

  it('renders with default small size', () => {
    render(<SkillBadge skill={mockSkill} />)
    
    const badge = screen.getByText('React')
    expect(badge).toHaveClass('text-xs')
    expect(badge).toHaveClass('px-2.5')
    expect(badge).toHaveClass('py-0.5')
  })

  it('renders with medium size', () => {
    render(<SkillBadge skill={mockSkill} size="md" />)
    
    const badge = screen.getByText('React')
    expect(badge).toHaveClass('text-sm')
    expect(badge).toHaveClass('px-3')
    expect(badge).toHaveClass('py-1')
  })

  it('renders with large size', () => {
    render(<SkillBadge skill={mockSkill} size="lg" />)
    
    const badge = screen.getByText('React')
    expect(badge).toHaveClass('text-base')
    expect(badge).toHaveClass('px-4')
    expect(badge).toHaveClass('py-1.5')
  })

  it('has rounded-full class for pill shape', () => {
    render(<SkillBadge skill={mockSkill} />)
    
    const badge = screen.getByText('React')
    expect(badge).toHaveClass('rounded-full')
  })

  it('is clickable when onClick is provided', () => {
    const handleClick = vi.fn()
    render(<SkillBadge skill={mockSkill} onClick={handleClick} />)
    
    const badge = screen.getByRole('button')
    fireEvent.click(badge)
    
    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('has cursor-pointer class when clickable', () => {
    const handleClick = vi.fn()
    render(<SkillBadge skill={mockSkill} onClick={handleClick} />)
    
    const badge = screen.getByRole('button')
    expect(badge).toHaveClass('cursor-pointer')
  })

  it('handles keyboard enter press when clickable', () => {
    const handleClick = vi.fn()
    render(<SkillBadge skill={mockSkill} onClick={handleClick} />)
    
    const badge = screen.getByRole('button')
    fireEvent.keyDown(badge, { key: 'Enter' })
    
    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('shows remove button when onRemove is provided', () => {
    const handleRemove = vi.fn()
    render(<SkillBadge skill={mockSkill} onRemove={handleRemove} />)
    
    const removeButton = screen.getByRole('button', { name: /Remove React/i })
    expect(removeButton).toBeInTheDocument()
  })

  it('calls onRemove when remove button is clicked', () => {
    const handleRemove = vi.fn()
    render(<SkillBadge skill={mockSkill} onRemove={handleRemove} />)
    
    const removeButton = screen.getByRole('button', { name: /Remove React/i })
    fireEvent.click(removeButton)
    
    expect(handleRemove).toHaveBeenCalledTimes(1)
  })

  it('does not trigger onClick when remove button is clicked', () => {
    const handleClick = vi.fn()
    const handleRemove = vi.fn()
    render(<SkillBadge skill={mockSkill} onClick={handleClick} onRemove={handleRemove} />)
    
    const removeButton = screen.getByRole('button', { name: /Remove React/i })
    fireEvent.click(removeButton)
    
    expect(handleRemove).toHaveBeenCalledTimes(1)
    expect(handleClick).not.toHaveBeenCalled()
  })

  it('shows tooltip when showTooltip is true', () => {
    render(<SkillBadge skill={mockSkill} showTooltip />)
    
    // Tooltip shows description (category)
    expect(screen.getByText('Technology')).toBeInTheDocument()
  })

  it('applies category-based color classes', () => {
    render(<SkillBadge skill={mockSkill} />)
    
    const badge = screen.getByText('React')
    // Should have background and text color classes
    expect(badge.className).toContain('bg-')
    expect(badge.className).toContain('text-')
  })
})

describe('SkillBadgeList', () => {
  it('renders nothing when skills array is empty', () => {
    const { container } = render(<SkillBadgeList skills={[]} />)
    
    expect(container.firstChild).toBeNull()
  })

  it('renders all skills when count is less than maxVisible', () => {
    const twoSkills = mockSkills.slice(0, 2)
    render(<SkillBadgeList skills={twoSkills} maxVisible={3} />)
    
    expect(screen.getByText('React')).toBeInTheDocument()
    expect(screen.getByText('Node.js')).toBeInTheDocument()
  })

  it('limits visible skills to maxVisible', () => {
    render(<SkillBadgeList skills={mockSkills} maxVisible={3} />)
    
    expect(screen.getByText('React')).toBeInTheDocument()
    expect(screen.getByText('Node.js')).toBeInTheDocument()
    expect(screen.getByText('Leadership')).toBeInTheDocument()
    // Should not show 4th and 5th skills
    expect(screen.queryByText('Communication')).not.toBeInTheDocument()
    expect(screen.queryByText('Teaching')).not.toBeInTheDocument()
  })

  it('shows "+N more" indicator when more skills than maxVisible', () => {
    render(<SkillBadgeList skills={mockSkills} maxVisible={3} />)
    
    // Should show +2 more
    expect(screen.getByText('+2 more')).toBeInTheDocument()
  })

  it('uses default maxVisible of 3', () => {
    render(<SkillBadgeList skills={mockSkills} />)
    
    // Default maxVisible is 3, should show +2 more
    expect(screen.getByText('+2 more')).toBeInTheDocument()
  })

  it('passes size prop to SkillBadge components', () => {
    render(<SkillBadgeList skills={mockSkills.slice(0, 2)} size="md" />)
    
    const badge = screen.getByText('React')
    expect(badge).toHaveClass('text-sm')
  })

  it('calls onMoreClick when +N more is clicked', () => {
    const handleMoreClick = vi.fn()
    render(<SkillBadgeList skills={mockSkills} maxVisible={3} onMoreClick={handleMoreClick} />)
    
    const moreButton = screen.getByText('+2 more')
    fireEvent.click(moreButton)
    
    expect(handleMoreClick).toHaveBeenCalledTimes(1)
  })
})
