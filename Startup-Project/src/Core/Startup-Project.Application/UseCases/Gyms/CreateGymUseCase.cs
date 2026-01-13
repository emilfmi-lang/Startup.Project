using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Interfaces;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.UseCases.Gyms;

public class CreateGymUseCase(IGymRepository gymRepository)
{
    public async Task<Guid> ExecuteAsync(CreateGymRequestDto dto, Guid ownerId)
    {
        var gym = new Gym(dto.Name, dto.Address, ownerId);
        await gymRepository.AddAsync(gym);
        return gym.Id;
    }
}
