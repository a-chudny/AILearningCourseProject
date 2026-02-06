import type { Skill } from '@/types';
import { getSkillColor } from '@/utils/skillColors';

interface SkillBadgeProps {
  skill: Skill;
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
  /** Show tooltip with category on hover */
  showTooltip?: boolean;
  /** Optional click handler */
  onClick?: () => void;
  /** Optional remove handler */
  onRemove?: () => void;
}

/**
 * Skill badge component with category-based color coding
 * Displays skill name with color based on category
 */
export function SkillBadge({
  skill,
  size = 'sm',
  showTooltip = false,
  onClick,
  onRemove,
}: SkillBadgeProps) {
  const colors = getSkillColor(skill.category);

  const sizeClasses = {
    sm: 'px-2.5 py-0.5 text-xs',
    md: 'px-3 py-1 text-sm',
    lg: 'px-4 py-1.5 text-base',
  };

  const baseClasses = `inline-flex items-center gap-1 rounded-full font-medium transition-colors ${colors.bg} ${colors.text} ${sizeClasses[size]}`;
  const interactiveClasses = onClick ? `cursor-pointer ${colors.hoverBg}` : '';

  const badge = (
    <span
      className={`${baseClasses} ${interactiveClasses}`}
      onClick={onClick}
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
      onKeyDown={onClick ? (e) => e.key === 'Enter' && onClick() : undefined}
    >
      {skill.name}
      {onRemove && (
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onRemove();
          }}
          className={`${colors.text} hover:opacity-70`}
          aria-label={`Remove ${skill.name}`}
        >
          <svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>
      )}
    </span>
  );

  if (showTooltip) {
    return (
      <span className="group relative inline-block">
        {badge}
        <span className="invisible absolute left-1/2 bottom-full mb-2 -translate-x-1/2 whitespace-nowrap rounded-md bg-gray-900 px-3 py-1.5 text-xs text-white opacity-0 transition-all group-hover:visible group-hover:opacity-100 z-10">
          {skill.category}
        </span>
        <span className="invisible absolute left-1/2 bottom-full -mb-1 -translate-x-1/2 border-4 border-transparent border-t-gray-900 opacity-0 group-hover:visible group-hover:opacity-100"></span>
      </span>
    );
  }

  return badge;
}

/**
 * Compact display of multiple skills with "+N more" indicator
 */
interface SkillBadgeListProps {
  skills: Skill[];
  maxVisible?: number;
  size?: 'sm' | 'md' | 'lg';
  showTooltips?: boolean;
  onMoreClick?: () => void;
}

export function SkillBadgeList({
  skills,
  maxVisible = 3,
  size = 'sm',
  showTooltips = false,
  onMoreClick,
}: SkillBadgeListProps) {
  if (skills.length === 0) return null;

  const visibleSkills = skills.slice(0, maxVisible);
  const remainingCount = Math.max(0, skills.length - maxVisible);

  return (
    <div className="flex flex-wrap gap-1.5">
      {visibleSkills.map((skill) => (
        <SkillBadge key={skill.id} skill={skill} size={size} showTooltip={showTooltips} />
      ))}
      {remainingCount > 0 && (
        <span
          className={`inline-flex items-center rounded-full bg-gray-100 text-gray-600 font-medium ${
            onMoreClick ? 'cursor-pointer hover:bg-gray-200' : ''
          } ${size === 'sm' ? 'px-2.5 py-0.5 text-xs' : size === 'md' ? 'px-3 py-1 text-sm' : 'px-4 py-1.5 text-base'}`}
          onClick={onMoreClick}
          role={onMoreClick ? 'button' : undefined}
          tabIndex={onMoreClick ? 0 : undefined}
          onKeyDown={onMoreClick ? (e) => e.key === 'Enter' && onMoreClick() : undefined}
          title={`${skills
            .slice(maxVisible)
            .map((s) => s.name)
            .join(', ')}`}
        >
          +{remainingCount} more
        </span>
      )}
    </div>
  );
}
