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

        CreateMap<Package, PackageDetailDto>()
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src =>
                src.Features
                    .Where(f => !f.IsDeleted)
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => f.Name)
                    .ToList()))
            .ForMember(dest => dest.DurationLabel, opt => opt.MapFrom(src => $"{src.DurationInDays} days"))
            .ForMember(dest => dest.PriceLabel, opt => opt.MapFrom(src => $"₼{src.Price:0}"));

        CreateMap<PackageFeature, PackageFeatureDto>();
    }
}
