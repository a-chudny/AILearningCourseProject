using AutoMapper;
using VolunteerPortal.API.Models.DTOs;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Models.DTOs.Registrations;
using VolunteerPortal.API.Models.Entities;

namespace VolunteerPortal.API.Application.Mapping;

/// <summary>
/// AutoMapper profile for core domain entities.
/// </summary>
public class DomainMappingProfile : Profile
{
    public DomainMappingProfile()
    {
        // Skill mappings
        CreateMap<Skill, SkillResponse>();

        // User mappings
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (int)src.Role))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => 
                src.UserSkills.Select(us => us.Skill)));

        // Registration mappings  
        CreateMap<Registration, RegistrationResponse>()
            .ForMember(dest => dest.Event, opt => opt.MapFrom(src => src.Event));

        CreateMap<Event, EventSummary>();

        CreateMap<Registration, EventRegistrationResponse>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

        CreateMap<User, UserSummary>();
    }
}
