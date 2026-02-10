using AutoMapper;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.GymOpeningHour;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Application.Services.Concretes;

public class GymOpeningHourService(
    IGenericRepository<GymOpeningHour> gymOpeningHourRepository,
    IGenericRepository<Gym> gymRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork
    ) : IGymOpeningHourService
{
    public async Task<ResponseModel<bool>> SetOpeningHoursAsync(Guid gymId, List<OpeningHourDto> dtos)
    {
        var gymExists = await gymRepository.AnyAsync(x => x.Id == gymId);
        if (!gymExists)
            throw NotFoundException.Gym(gymId);

        var oldHours = await gymOpeningHourRepository.GetWhereAsync(x => x.GymId == gymId);
        foreach (var hour in oldHours)
            gymOpeningHourRepository.Remove(hour);

        foreach (var dto in dtos)
        {
            var newHour = mapper.Map<GymOpeningHour>(dto);
            newHour.GymId = gymId;
            await gymOpeningHourRepository.AddAsync(newHour);
        }

        var affectedRows = await unitOfWork.SaveChangesAsync();
        if (affectedRows <= 0)
            throw BusinessException.Custom(BusinessErrorCodes.UnknownError, "Məlumatlar yadda saxlanılmadı.");

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<List<OpeningHourDto>>> GetOpeningHoursAsync(Guid gymId)
    {
        var gymExists = await gymRepository.AnyAsync(x => x.Id == gymId);
        if (!gymExists)
            throw NotFoundException.Gym(gymId);

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
