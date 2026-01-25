

using AutoMapper;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.GymOpeningHour;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Services.Concretes;

public class GymOpeningHourService(
    IGenericRepository<GymOpeningHour> gymOpeningHourRepository,
    IGenericRepository<Gym> gymRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork
    ): IGymOpeningHourService
{
    public async Task<ResponseModel<bool>> SetOpeningHoursAsync(Guid gymId, List<OpeningHourDto> dtos)
    {
        var gymExists = await gymRepository.AnyAsync(x => x.Id == gymId);
        if (!gymExists)
            return ResponseModel<bool>.FailureResponse("Zal tapılmadı.");

        var oldHours = await gymOpeningHourRepository.GetWhereAsync(x => x.GymId == gymId);
        if (oldHours.Any())
        {
            foreach (var hour in oldHours)
            {
                gymOpeningHourRepository.Remove(hour);
            }
        }

        foreach (var dto in dtos)
        {
            var newHour = mapper.Map<GymOpeningHour>(dto);
            newHour.GymId = gymId;

            await gymOpeningHourRepository.AddAsync(newHour);
        }
        var affectedRows = await unitOfWork.SaveChangesAsync();

        return affectedRows > 0
            ? ResponseModel<bool>.SuccessResponse(true)
            : ResponseModel<bool>.FailureResponse("Xəta baş verdi: Məlumatlar yadda saxlanılmadı.");
    }

}
