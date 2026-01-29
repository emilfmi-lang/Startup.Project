using AutoMapper;
using Startup_Project.Application.Dtos.TrainingSession;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Mappings.TrainingSessions;

public class TrainingSessionProfile : Profile
{
    public TrainingSessionProfile()
    {
        CreateMap<TrainingSession, SessionDto>()
            .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => src.Trainer.User.FullName))
            .ForMember(dest => dest.TrainerSpecialization, opt => opt.MapFrom(src => src.Trainer.Specialization))
            .ForMember(dest => dest.TrainerProfileImage, opt => opt.MapFrom(src => src.Trainer.ProfileImageUrl))
            .ForMember(dest => dest.GymName, opt => opt.MapFrom(src => src.Gym != null ? src.Gym.Name : "Online"))
            .ForMember(dest => dest.GymAddress, opt => opt.MapFrom(src => src.Gym != null ? src.Gym.Address : "Online Session"))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired()))
            .ForMember(dest => dest.CanBeUsed, opt => opt.MapFrom(src => src.Status == SessionStatus.Paid && !src.IsExpired()))
            .ForMember(dest => dest.DaysUntilExpiry, opt => opt.MapFrom(src =>
                src.ExpiresAt.HasValue && src.Status == SessionStatus.Paid
                    ? Math.Max(0, (int)(src.ExpiresAt.Value - DateTime.UtcNow).TotalDays)
                    : (int?)null));
    }
}
