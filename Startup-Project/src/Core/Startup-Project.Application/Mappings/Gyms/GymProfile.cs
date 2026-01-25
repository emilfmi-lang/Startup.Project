

using AutoMapper;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Dtos.GymOpeningHour;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Gyms;

public class GymProfile:Profile
{
    public GymProfile()
    {
        CreateMap<Gym, GymGetDto>().ForMember(dest => dest.OwnerFullName, opt => opt.MapFrom(src => src.Owner.FullName));
        CreateMap<Gym, GymListDto>()
            .ForMember(dest => dest.IsOpen, opt => opt.MapFrom(src => src.IsOpenNow()));
        CreateMap<OpeningHourDto, GymOpeningHour>()
            .ForMember(dest => dest.OpenTime,
                       opt => opt.MapFrom(src => TimeSpan.Parse(src.OpenTime)))
            .ForMember(dest => dest.ClosingTime,
                       opt => opt.MapFrom(src => TimeSpan.Parse(src.ClosingTime)));
    }
}
