using AutoMapper;
using Startup_Project.Application.Dtos.Notification;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Notification;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Domain.Entities.Notification, NotificationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Channel, opt => opt.MapFrom(src => src.Channel.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead));
    }
}
