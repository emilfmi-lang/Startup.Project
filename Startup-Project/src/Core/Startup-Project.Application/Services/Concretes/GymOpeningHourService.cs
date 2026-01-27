

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
            return ResponseModel<bool>.FailureResponse("Zal tapilmadi.");

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
            : ResponseModel<bool>.FailureResponse("Xeta bas verdi: Melumatlar yadda saxlanilmadi.");
    }

    public async Task<ResponseModel<List<OpeningHourDto>>> GetOpeningHoursAsync(Guid gymId)
    {
        var gymExists = await gymRepository.AnyAsync(x => x.Id == gymId);
        if (!gymExists)
            return ResponseModel<List<OpeningHourDto>>.FailureResponse("Zal tapilmadi.");

        var hours = await gymOpeningHourRepository.GetWhereAsync(x => x.GymId == gymId);

        var dtos = hours.Select(h => new OpeningHourDto
        {
            DayOfWeek = h.DayOfWeek,
            OpenTime = h.OpenTime.ToString(@"hh\:mm"),
            ClosingTime = h.ClosingTime.ToString(@"hh\:mm"),
            IsClosed = h.IsClosed
        }).ToList();

        return ResponseModel<List<OpeningHourDto>>.SuccessResponse(dtos);
    }
}
