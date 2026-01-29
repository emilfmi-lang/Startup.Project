using AutoMapper;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Dtos.TrainerReview;
using Startup_Project.Application.Dtos.TrainerSchedule;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Trainers;

public class TrainerProfile : Profile
{
    public TrainerProfile()
    {
        // Trainer -> TrainerDto
        CreateMap<Trainer, TrainerDto>()
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.GymName, opt => opt.MapFrom(src => src.Gym != null ? src.Gym.Name : "Online"))
            .ForMember(dest => dest.GymId, opt => opt.MapFrom(src => src.GymId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Trainer -> TrainerListDto
        CreateMap<Trainer, TrainerListDto>()
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.GymName, opt => opt.MapFrom(src => src.Gym != null ? src.Gym.Name : "Online"));

        // TrainerReview -> TrainerReviewDto
        CreateMap<TrainerReview, TrainerReviewDto>()
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));

        // TrainerSchedule -> TrainerScheduleDto
        CreateMap<TrainerSchedule, TrainerScheduleDto>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")));
    }
}
