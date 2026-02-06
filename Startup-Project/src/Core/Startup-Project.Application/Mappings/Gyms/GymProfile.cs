using AutoMapper;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Dtos.GymOpeningHour;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Gyms;

public class GymProfile : Profile
{
    public GymProfile()
    {
        CreateMap<Gym, GymGetDto>()
            .ForMember(dest => dest.OwnerFullName, opt => opt.MapFrom(src => src.Owner.FullName));

        CreateMap<Gym, GymListDto>()
            .ForMember(dest => dest.IsOpen, opt => opt.MapFrom(src => src.IsOpenNow()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => 
                src.Amenities
                    .Where(a => a.IsHighlighted && !a.IsDeleted)
                    .OrderBy(a => a.DisplayOrder)
                    .Take(4)
                    .Select(a => a.AmenityType.ToString())
                    .ToList()))
            .ForMember(dest => dest.MinMonthlyPrice, opt => opt.MapFrom(src =>
                src.Packages
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.Price)
                    .Select(p => (decimal?)p.Price)
                    .FirstOrDefault()));

        CreateMap<Gym, GymCardDto>()
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImagePath))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.AverageRating))
            .ForMember(dest => dest.IsOpen, opt => opt.MapFrom(src => src.IsOpenNow()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                src.Amenities
                    .Where(a => a.IsHighlighted && !a.IsDeleted)
                    .OrderBy(a => a.DisplayOrder)
                    .Take(4)
                    .Select(a => a.AmenityType.ToString())
                    .ToList()))
            .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src =>
                src.Packages
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.Price)
                    .Select(p => (decimal?)p.Price)
                    .FirstOrDefault()))
            .ForMember(dest => dest.PriceLabel, opt => opt.MapFrom(src =>
                src.Packages.Any(p => !p.IsDeleted)
                    ? $"from ₼{src.Packages.Where(p => !p.IsDeleted).Min(p => p.Price)}/mo"
                    : null));

        CreateMap<GymAmenity, GymAmenityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.AmenityType.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GetDisplayName()));

        CreateMap<OpeningHourDto, GymOpeningHour>()
            .ForMember(dest => dest.OpenTime,
                       opt => opt.MapFrom(src => TimeSpan.Parse(src.OpenTime)))
            .ForMember(dest => dest.ClosingTime,
                       opt => opt.MapFrom(src => TimeSpan.Parse(src.ClosingTime)));
    }
}
