using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Services;

/// <summary>
/// Service for managing skills and user skill associations.
/// </summary>
public class SkillService : ISkillService
{
    private readonly ApplicationDbContext _context;

    public SkillService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SkillResponse>> GetAllSkillsAsync()
    {
        var skills = await _context.Skills
            .OrderBy(s => s.Name)
            .ToListAsync();

        return skills.Select(s => new SkillResponse
        {
            Id = s.Id,
            Name = s.Name,
            Category = s.Description ?? "General"
        }).ToList();
    }

    public async Task<List<SkillResponse>> GetUserSkillsAsync(int userId)
    {
        var userSkills = await _context.UserSkills
            .Include(us => us.Skill)
            .Where(us => us.UserId == userId)
            .OrderBy(us => us.Skill.Name)
            .ToListAsync();

        return userSkills.Select(us => new SkillResponse
        {
            Id = us.Skill.Id,
            Name = us.Skill.Name,
            Category = us.Skill.Description ?? "General"
        }).ToList();
    }

    public async Task UpdateUserSkillsAsync(int userId, List<int> skillIds)
    {
        // Validate all skill IDs exist
        var existingSkills = await _context.Skills
            .Where(s => skillIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        var invalidSkillIds = skillIds.Except(existingSkills).ToList();
        if (invalidSkillIds.Any())
        {
            throw new ArgumentException($"Invalid skill IDs: {string.Join(", ", invalidSkillIds)}");
        }

        // Remove all existing user skills
        var existingUserSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .ToListAsync();

        _context.UserSkills.RemoveRange(existingUserSkills);

        // Add new user skills (remove duplicates from input)
        var uniqueSkillIds = skillIds.Distinct().ToList();
        var newUserSkills = uniqueSkillIds.Select(skillId => new UserSkill
        {
            UserId = userId,
            SkillId = skillId
        }).ToList();

        await _context.UserSkills.AddRangeAsync(newUserSkills);
        await _context.SaveChangesAsync();
    }
}
