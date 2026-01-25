using AutoMapper;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Payments;

public class PaymentProfile:Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentRequestDto, Payment>();
        CreateMap<Package, Subscription>()
            .ForMember(dest => dest.PackageId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateTime.UtcNow.AddDays(src.DurationInDays)));
    }
}
