

using AutoMapper;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Services.Concretes;

public class GymOpeningHourService(
    IGenericRepository<GymOpeningHour> gymOpeningHourRepository,
    IGenericRepository<Gym> gymRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork
    )
{
}
