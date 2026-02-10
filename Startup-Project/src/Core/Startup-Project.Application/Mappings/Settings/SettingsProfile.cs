using AutoMapper;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Application.Mappings.Settings;

public class SettingsProfile : Profile
{
    public SettingsProfile()
    {
        // AppUser -> NotificationSettingsDto
        CreateMap<AppUser, NotificationSettingsDto>()
            .ForMember(dest => dest.PushNotifications, opt => opt.MapFrom(src => src.NotifyPush))
            .ForMember(dest => dest.EmailNotifications, opt => opt.MapFrom(src => src.NotifyEmail))
            .ForMember(dest => dest.SmsNotifications, opt => opt.MapFrom(src => src.NotifySms))
            .ForMember(dest => dest.WorkoutReminders, opt => opt.MapFrom(src => src.NotifyWorkoutReminders))
            .ForMember(dest => dest.PromotionalOffers, opt => opt.MapFrom(src => src.NotifyPromotions))
            .ForMember(dest => dest.SessionUpdates, opt => opt.MapFrom(src => src.NotifySessionUpdates))
            .ForMember(dest => dest.NewFeatures, opt => opt.MapFrom(src => src.NotifyNewFeatures));

        // AppUser -> UserPreferencesDto
        CreateMap<AppUser, UserPreferencesDto>()
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.Theme, opt => opt.MapFrom(src => src.Theme))
            .ForMember(dest => dest.DistanceUnit, opt => opt.MapFrom(src => src.DistanceUnit))
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone));

            // UserSession -> ActiveSessionDto
            CreateMap<UserSession, ActiveSessionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsCurrent ? "Current" : "Revoke"))
                .ForMember(dest => dest.LastActiveLabel, opt => opt.Ignore());

            // UserPaymentMethod -> PaymentMethodDto
            CreateMap<UserPaymentMethod, PaymentMethodDto>()
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.CardType.ToString()))
                .ForMember(dest => dest.MaskedNumber, opt => opt.MapFrom(src => $"•••• •••• •••• {src.Last4Digits}"))
                .ForMember(dest => dest.ExpiryLabel, opt => opt.MapFrom(src => $"{src.ExpiryMonth:D2}/{src.ExpiryYear % 100:D2}"))
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired()));
        }
}
