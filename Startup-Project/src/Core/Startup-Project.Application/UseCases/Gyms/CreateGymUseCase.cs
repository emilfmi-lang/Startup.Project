using AutoMapper;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;
using Startup_Project.Domain.Interfaces;

namespace Startup_Project.Application.UseCases.Gyms;

public class CreateGymUseCase(IGymRepository repository,IMapper mapper)
{
    public async Task<Guid> ExecuteAsync(CreateGymRequestDto dto)
    {
        var existingGym = await repository.ExistsByNameAsync(dto.Name);
        if (existingGym != null)
            throw new Exception("Gym with this name already exists");
        var gym = mapper.Map<Gym>(dto);
        await repository.AddAsync(gym);
        return gym.Id;
    }
}
