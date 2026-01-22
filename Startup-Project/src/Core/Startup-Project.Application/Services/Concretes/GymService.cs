using Microsoft.AspNetCore.Http;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using System.Security.Claims;


namespace Startup_Project.Application.Services.Concretes;

public class GymService(IGenericRepository<Gym> repository,
                        IFileService fileService,
                         IHttpContextAccessor httpContextAccessor ) : IGymService
{
}
