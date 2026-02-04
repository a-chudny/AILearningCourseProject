import { useState, useEffect } from 'react'
import { useAuth } from '@/hooks/useAuth'
import { useSkills, useUserSkills, useUpdateUserSkills } from '@/hooks/useSkills'
import { SkillSelector } from '@/components/skills/SkillSelector'

/**
 * User profile page with basic info and skill management
 * Shows read-only profile info (name, email, role) and allows users to manage their skills
 */
export default function ProfilePage() {
  const { user } = useAuth()
  const { data: allSkills, isLoading: isLoadingAllSkills, error: allSkillsError } = useSkills()
  const { data: userSkills, isLoading: isLoadingUserSkills, error: userSkillsError } = useUserSkills()
  const updateSkillsMutation = useUpdateUserSkills()

  // Local state for selected skill IDs
  const [selectedSkillIds, setSelectedSkillIds] = useState<number[]>([])
  const [hasChanges, setHasChanges] = useState(false)
  const [showSuccessMessage, setShowSuccessMessage] = useState(false)

  // Initialize selected skills from user's current skills
  useEffect(() => {
    if (userSkills) {
      const ids = userSkills.map((skill) => skill.id)
      setSelectedSkillIds(ids)
    }
  }, [userSkills])

  // Track if there are unsaved changes
  useEffect(() => {
    if (userSkills) {
      const currentIds = new Set(userSkills.map((s) => s.id))
      const selectedIds = new Set(selectedSkillIds)
      const hasChanged =
        currentIds.size !== selectedIds.size ||
        Array.from(currentIds).some((id) => !selectedIds.has(id))
      setHasChanges(hasChanged)
    }
  }, [selectedSkillIds, userSkills])

  const handleSave = async () => {
    try {
      await updateSkillsMutation.mutateAsync(selectedSkillIds)
      setHasChanges(false)
      setShowSuccessMessage(true)
      
      // Hide success message after 3 seconds
      setTimeout(() => {
        setShowSuccessMessage(false)
      }, 3000)
    } catch (error) {
      console.error('Failed to update skills:', error)
    }
  }

  const handleCancel = () => {
    if (userSkills) {
      setSelectedSkillIds(userSkills.map((skill) => skill.id))
      setHasChanges(false)
    }
  }

  if (!user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-gray-600">Please log in to view your profile.</p>
      </div>
    )
  }

  const isLoading = isLoadingAllSkills || isLoadingUserSkills
  const error = allSkillsError || userSkillsError

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
        {/* Page Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">My Profile</h1>
          <p className="mt-2 text-gray-600">Manage your profile information and skills</p>
        </div>

        {/* Profile Information Card */}
        <div className="bg-white shadow rounded-lg p-6 mb-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Profile Information</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-500">Name</label>
              <p className="mt-1 text-base text-gray-900">{user.name}</p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-500">Email</label>
              <p className="mt-1 text-base text-gray-900">{user.email}</p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-500">Role</label>
              <p className="mt-1">
                <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800">
                  {user.role}
                </span>
              </p>
            </div>
          </div>
        </div>

        {/* Skills Management Card */}
        <div className="bg-white shadow rounded-lg p-6">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="text-xl font-semibold text-gray-900">My Skills</h2>
              <p className="mt-1 text-sm text-gray-600">
                Select your skills to help organizers match you with relevant volunteer opportunities
              </p>
            </div>
          </div>

          {/* Success Message */}
          {showSuccessMessage && (
            <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-2">
              <svg
                className="h-5 w-5 text-green-600"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M5 13l4 4L19 7"
                />
              </svg>
              <p className="text-sm text-green-800 font-medium">Skills updated successfully!</p>
            </div>
          )}

          {/* Error Message */}
          {error && (
            <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-sm text-red-800">
                Failed to load skills. Please try again later.
              </p>
            </div>
          )}

          {/* Loading State */}
          {isLoading && (
            <div className="flex items-center justify-center py-12">
              <div className="flex flex-col items-center gap-4">
                <div className="h-12 w-12 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
                <p className="text-gray-600">Loading skills...</p>
              </div>
            </div>
          )}

          {/* Skill Selector */}
          {!isLoading && !error && allSkills && (
            <>
              <div className="mb-6">
                <SkillSelector
                  allSkills={allSkills}
                  selectedSkillIds={selectedSkillIds}
                  onChange={setSelectedSkillIds}
                  label="Select your skills"
                />
              </div>

              {/* Action Buttons */}
              {hasChanges && (
                <div className="flex items-center gap-3 pt-4 border-t border-gray-200">
                  <button
                    type="button"
                    onClick={handleSave}
                    disabled={updateSkillsMutation.isPending}
                    className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-blue-400 disabled:cursor-not-allowed transition-colors font-medium"
                  >
                    {updateSkillsMutation.isPending ? 'Saving...' : 'Save Changes'}
                  </button>
                  <button
                    type="button"
                    onClick={handleCancel}
                    disabled={updateSkillsMutation.isPending}
                    className="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 disabled:bg-gray-100 disabled:cursor-not-allowed transition-colors font-medium"
                  >
                    Cancel
                  </button>
                  <p className="text-sm text-amber-600 flex items-center gap-1">
                    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                      />
                    </svg>
                    You have unsaved changes
                  </p>
                </div>
              )}

              {/* Mutation Error */}
              {updateSkillsMutation.isError && (
                <div className="mt-4 p-4 bg-red-50 border border-red-200 rounded-lg">
                  <p className="text-sm text-red-800">
                    Failed to update skills: {updateSkillsMutation.error?.message || 'Unknown error'}
                  </p>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}
