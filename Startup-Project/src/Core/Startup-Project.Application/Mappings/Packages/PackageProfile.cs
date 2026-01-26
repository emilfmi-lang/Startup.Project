
using AutoMapper;
using Startup_Project.Application.Dtos.Packages;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Packages;
public class PackageProfile : Profile
{
    public PackageProfile()
    {
        CreateMap<Package, PackageResponseDto>()
            .ForMember(dest => dest.GymName, opt => opt.MapFrom(src => src.Gym.Name));
    }
}
