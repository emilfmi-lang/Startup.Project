
using AutoMapper;
using Startup_Project.Application.Dtos.GymDtos.GymReview;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Gyms;

public class ReviewProfile:Profile
{
    public ReviewProfile()
    {
        CreateMap<GymReview, ReviewGetDto>()
            .ForMember(dest => dest.UserFullName,
                       opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.CreatedAt,
                       opt => opt.MapFrom(src => src.CreatedAt));
    }
}
