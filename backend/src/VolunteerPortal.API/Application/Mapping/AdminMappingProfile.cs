using AutoMapper;
using VolunteerPortal.API.Application.Admin.Models;
using VolunteerPortal.API.Application.Common;
using VolunteerPortal.API.Models.DTOs.Admin;
using VolunteerPortal.API.Models.Entities;

namespace VolunteerPortal.API.Application.Mapping;

/// <summary>
/// AutoMapper profile for Admin domain mappings.
/// </summary>
public class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        // Entity to Application Model
        CreateMap<User, AdminUserModel>();

        // Application Model to DTO
        CreateMap<AdminUserModel, AdminUserResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (int)src.Role))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.ToString()));

        CreateMap<AdminStatsModel, AdminStatsResponse>();

        // PagedResult mapping
        CreateMap<PagedResult<AdminUserModel>, AdminUserListResponse>()
            .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Items));
    }
}
