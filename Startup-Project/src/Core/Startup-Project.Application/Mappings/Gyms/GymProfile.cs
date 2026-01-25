

using AutoMapper;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Gyms;

public class GymProfile:Profile
{
    public GymProfile()
    {
        CreateMap<Gym, GymGetDto>().ForMember(dest => dest.OwnerFullName, opt => opt.MapFrom(src => src.Owner.FullName));
        
    }
}
