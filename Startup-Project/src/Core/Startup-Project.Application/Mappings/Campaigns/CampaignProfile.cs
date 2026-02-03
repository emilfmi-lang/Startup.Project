using AutoMapper;
using Startup_Project.Application.Dtos.Campaign;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Campaigns;

public class CampaignProfile : Profile
{
    public CampaignProfile()
    {
        CreateMap<Campaign, CampaignListDto>()
            .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src => src.DiscountType.ToString()))
            .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src => src.TargetType.ToString()));

        CreateMap<Campaign, CampaignDetailDto>()
            .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src => src.DiscountType.ToString()))
            .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src => src.TargetType.ToString()))
            .ForMember(dest => dest.Rules, opt => opt.MapFrom(src => src.Rules))
            .ForMember(dest => dest.RecentUsages, opt => opt.Ignore()); // Manual mapping required for user details

        CreateMap<CampaignRule, CampaignRuleDto>()
            .ForMember(dest => dest.RuleType, opt => opt.MapFrom(src => src.RuleType.ToString()))
            .ForMember(dest => dest.Operator, opt => opt.MapFrom(src => src.Operator.ToString()));

        CreateMap<CampaignUsage, CampaignUsageDto>()
            .ForMember(dest => dest.UserFullName, opt => opt.Ignore())
            .ForMember(dest => dest.UserEmail, opt => opt.Ignore())
            .ForMember(dest => dest.PackageName, opt => opt.Ignore());
    }
}
