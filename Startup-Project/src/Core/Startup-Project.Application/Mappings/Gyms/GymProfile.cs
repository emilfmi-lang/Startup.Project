

using AutoMapper;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Mappings.Gyms;

public class GymProfile:Profile
{
    public GymProfile()
    {
        CreateMap<GymCreateDto, Gym>()
    .ConstructUsing(dto => new Gym(dto.Name, dto.Address, Guid.Empty, null));
    }
}
