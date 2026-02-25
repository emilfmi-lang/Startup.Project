
using AutoMapper;
using Startup_Project.Application.Dtos.GymDtos.GymReview;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Gyms;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<GymReview, ReviewGetDto>()
            .ForMember(dest => dest.UserFullName,
                       opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.UserEmail,
                       opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.UserProfileImageUrl,
                       opt => opt.MapFrom(src => src.User.ProfileImageUrl))
            .ForMember(dest => dest.CreatedAt,
                       opt => opt.MapFrom(src => src.CreatedAt));
        CreateMap<ReviewCreateDto, GymReview>();
    }
}
