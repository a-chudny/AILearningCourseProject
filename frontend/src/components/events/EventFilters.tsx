import { useState, useEffect } from 'react';
import { EventStatus } from '@/types/enums';
import { useSkills, useUserSkills } from '@/hooks/useSkills';
import { authService } from '@/services/authService';

interface EventFiltersProps {
  onFilterChange: (filters: {
    searchTerm?: string;
    status?: string;
    includePastEvents?: boolean;
    skillIds?: number[];
    matchMySkills?: boolean;
  }) => void;
  initialSearchTerm?: string;
  initialStatus?: string;
  initialIncludePastEvents?: boolean;
  initialSkillIds?: number[];
  initialMatchMySkills?: boolean;
}

export function EventFilters({
  onFilterChange,
  initialSearchTerm = '',
  initialStatus = '',
  initialIncludePastEvents = false,
  initialSkillIds = [],
  initialMatchMySkills = false,
}: EventFiltersProps) {
  const [searchTerm, setSearchTerm] = useState(initialSearchTerm);
  const [status, setStatus] = useState(initialStatus);
  const [includePastEvents, setIncludePastEvents] = useState(initialIncludePastEvents);
  const [selectedSkillIds, setSelectedSkillIds] = useState<number[]>(initialSkillIds);
  const [matchMySkills, setMatchMySkills] = useState(initialMatchMySkills);
  const [skillsDropdownOpen, setSkillsDropdownOpen] = useState(false);

  // Fetch all available skills
  const { data: allSkills = [], isLoading: skillsLoading } = useSkills();

  // Fetch user's skills (only if authenticated)
  const isAuthenticated = authService.isAuthenticated();
  const { data: userSkills = [] } = useUserSkills();

  // Debounce search term and trigger filter changes
  useEffect(() => {
    const timer = setTimeout(() => {
      onFilterChange({
        searchTerm: searchTerm || undefined,
        status: status || undefined,
        includePastEvents,
        skillIds: selectedSkillIds.length > 0 ? selectedSkillIds : undefined,
        matchMySkills,
      });
    }, 300); // 300ms debounce

    return () => clearTimeout(timer);
  }, [searchTerm, status, includePastEvents, selectedSkillIds, matchMySkills, onFilterChange]);

  const handleSkillToggle = (skillId: number) => {
    setSelectedSkillIds((prev) =>
      prev.includes(skillId) ? prev.filter((id) => id !== skillId) : [...prev, skillId]
    );
  };

  const handleMatchMySkillsToggle = () => {
    const newValue = !matchMySkills;
    setMatchMySkills(newValue);

    // Clear manual skill selection when enabling "Match My Skills"
    if (newValue) {
      setSelectedSkillIds([]);
    }
  };

  const handleReset = () => {
    setSearchTerm('');
    setStatus('');
    setIncludePastEvents(false);
    setSelectedSkillIds([]);
    setMatchMySkills(false);
  };

  const hasActiveFilters = searchTerm || status || includePastEvents || selectedSkillIds.length > 0 || matchMySkills;

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {/* Search input */}
        <div className="lg:col-span-2">
          <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-1">
            Search Events
          </label>
          <input
            id="search"
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search by title or description..."
            className="w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 bg-white focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Status filter */}
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
            Event Status
          </label>
          <select
            id="status"
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            className="w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 bg-white focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Statuses</option>
            <option value={EventStatus.Active}>Active</option>
            <option value={EventStatus.Cancelled}>Cancelled</option>
          </select>
        </div>

        {/* Include past events checkbox */}
        <div className="flex items-end">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              checked={includePastEvents}
              onChange={(e) => setIncludePastEvents(e.target.checked)}
              className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-2 focus:ring-blue-500"
            />
            <span className="text-sm font-medium text-gray-700">Include past events</span>
          </label>
        </div>
      </div>

      {/* Skills filter section */}
      <div className="mt-4 grid gap-4 sm:grid-cols-2">
        {/* Skill multi-select dropdown */}
        <div className="relative">
          <label className="block text-sm font-medium text-gray-700 mb-1">Required Skills</label>
          <button
            type="button"
            onClick={() => setSkillsDropdownOpen(!skillsDropdownOpen)}
            disabled={matchMySkills || skillsLoading}
            className="w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-left focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed flex justify-between items-center"
          >
            <span className="truncate">
              {skillsLoading
                ? 'Loading skills...'
                : matchMySkills
                  ? 'Using your skills'
                  : selectedSkillIds.length > 0
                    ? `${selectedSkillIds.length} skill${selectedSkillIds.length === 1 ? '' : 's'} selected`
                    : 'All skills'}
            </span>
            <svg
              className={`w-4 h-4 transition-transform ${skillsDropdownOpen ? 'rotate-180' : ''}`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </button>

          {/* Dropdown menu */}
          {skillsDropdownOpen && !matchMySkills && (
            <div className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
              {allSkills.length === 0 ? (
                <div className="px-4 py-3 text-sm text-gray-500">No skills available</div>
              ) : (
                <div className="py-1">
                  {allSkills.map((skill) => (
                    <label
                      key={skill.id}
                      className="flex items-center gap-2 px-4 py-2 hover:bg-gray-50 cursor-pointer"
                    >
                      <input
                        type="checkbox"
                        checked={selectedSkillIds.includes(skill.id)}
                        onChange={() => handleSkillToggle(skill.id)}
                        className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-2 focus:ring-blue-500"
                      />
                      <span className="text-sm text-gray-700">
                        {skill.name}
                        <span className="text-xs text-gray-500 ml-1">({skill.description})</span>
                      </span>
                    </label>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Match my skills checkbox - only show if authenticated */}
        {isAuthenticated && (
          <div className="flex items-end">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={matchMySkills}
                onChange={handleMatchMySkillsToggle}
                disabled={userSkills.length === 0}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed"
              />
              <span className="text-sm font-medium text-gray-700">
                Match my skills
                {userSkills.length === 0 && (
                  <span className="text-xs text-gray-500 block">
                    (Add skills to your profile first)
                  </span>
                )}
              </span>
            </label>
          </div>
        )}
      </div>

      {/* Reset button */}
      {hasActiveFilters && (
        <div className="mt-4 flex justify-end">
          <button
            onClick={handleReset}
            className="text-sm font-medium text-blue-600 hover:text-blue-700 transition-colors"
          >
            Clear All Filters
          </button>
        </div>
      )}

      {/* Close dropdown when clicking outside */}
      {skillsDropdownOpen && (
        <div className="fixed inset-0 z-0" onClick={() => setSkillsDropdownOpen(false)} />
      )}
    </div>
  );
}
