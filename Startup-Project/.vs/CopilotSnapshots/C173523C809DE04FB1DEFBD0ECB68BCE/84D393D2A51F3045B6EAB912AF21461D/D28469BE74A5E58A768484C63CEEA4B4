

using AutoMapper;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Application.Mappings.Account;

public class AccountProfile:Profile
{
    public AccountProfile()
    {
        CreateMap<UserRegisterDto, AppUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Gyms, opt => opt.Ignore());
    }
}
