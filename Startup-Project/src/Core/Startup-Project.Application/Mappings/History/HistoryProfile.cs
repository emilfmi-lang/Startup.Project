using AutoMapper;
using Startup_Project.Application.Dtos.History;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.History;

public class HistoryProfile : Profile
{
    public HistoryProfile()
    {
        // CheckInLog -> TimelineActivityDto
        CreateMap<CheckInLog, TimelineActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => "gym-checkin"))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(_ => "Gym Check-in"))
            .ForMember(dest => dest.GymName, opt => opt.MapFrom(src => src.Gym != null ? src.Gym.Name : null))
            .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(_ => (string?)null))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.CheckInTime.ToString("HH:mm")))
            .ForMember(dest => dest.Icon, opt => opt.MapFrom(_ => "gym"))
            .ForMember(dest => dest.Duration, opt => opt.Ignore())
            .ForMember(dest => dest.DurationLabel, opt => opt.Ignore())
            .ForMember(dest => dest.Subtitle, opt => opt.Ignore());

        // TrainingSession -> TimelineActivityDto
        CreateMap<TrainingSession, TimelineActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => "training-session"))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src =>
                $"{(src.Trainer != null ? src.Trainer.Specialization : "Training")} Session"))
            .ForMember(dest => dest.GymName, opt => opt.MapFrom(src => src.Gym != null ? src.Gym.Name : null))
            .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src =>
                src.Trainer != null && src.Trainer.User != null ? src.Trainer.User.FullName : null))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src =>
                src.UsedAt.HasValue ? src.UsedAt.Value.ToString("HH:mm") : ""))
            .ForMember(dest => dest.Icon, opt => opt.MapFrom(_ => "trainer"))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(_ => 60))
            .ForMember(dest => dest.DurationLabel, opt => opt.MapFrom(_ => "60 min"))
            .ForMember(dest => dest.Subtitle, opt => opt.Ignore());
    }
}
