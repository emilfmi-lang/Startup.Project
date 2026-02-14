using AutoMapper;
using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Application.Mappings.Admin;

public class AdminMembersProfile : Profile
{
    public AdminMembersProfile()
    {
        // AppUser -> MemberListItemDto
        CreateMap<AppUser, MemberListItemDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfileImageUrl))
            .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.Plan, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiresDate, opt => opt.Ignore())
            .ForMember(dest => dest.CheckIns, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsTrainer, opt => opt.Ignore())
            .ForMember(dest => dest.LastCheckIn, opt => opt.Ignore());

        // AppUser -> MemberDetailDto
        CreateMap<AppUser, MemberDetailDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfileImageUrl))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
            .ForMember(dest => dest.Gym, opt => opt.Ignore())
            .ForMember(dest => dest.Subscription, opt => opt.Ignore())
            .ForMember(dest => dest.Stats, opt => opt.Ignore())
            .ForMember(dest => dest.Trainer, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        // Trainer -> TrainerListItemDto
        CreateMap<Trainer, TrainerListItemDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
            .ForMember(dest => dest.Experience, opt => opt.Ignore())
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => src.IsVerified))
            .ForMember(dest => dest.SessionCount, opt => opt.MapFrom(src => src.TotalSessions))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.AverageRating))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // Gym -> GymInfoDto
        CreateMap<Gym, GymInfoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
    }
}
