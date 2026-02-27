using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Dtos.GymOwner;
using Startup_Project.Application.Dtos.Packages;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class GymOwnerService : IGymOwnerService
{
    private readonly IGenericRepository<Gym> _gymRepository;
    private readonly IGenericRepository<Subscription> _subscriptionRepository;
    private readonly IGenericRepository<CheckInLog> _checkInLogRepository;
    private readonly IGenericRepository<Package> _packageRepository;
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public GymOwnerService(
        IGenericRepository<Gym> gymRepository,
        IGenericRepository<Subscription> subscriptionRepository,
        IGenericRepository<CheckInLog> checkInLogRepository,
        IGenericRepository<Package> packageRepository,
        IGenericRepository<Payment> paymentRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _gymRepository = gymRepository;
        _subscriptionRepository = subscriptionRepository;
        _checkInLogRepository = checkInLogRepository;
        _packageRepository = packageRepository;
        _paymentRepository = paymentRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw BusinessException.UnauthorizedAccess();
        return userId;
    }

    private async Task<Gym> GetCurrentUserGymAsync()
    {
        var userId = GetCurrentUserId();
        var gym = await _gymRepository.Table
            .Include(g => g.OpeningHours)
            .FirstOrDefaultAsync(g => g.OwnerId == userId && !g.IsDeleted);

        if (gym == null)
            throw NotFoundException.GymForOwner();

        return gym;
    }

    public async Task<ResponseModel<MyGymDetailDto>> GetMyGymAsync()
    {
        var gym = await GetCurrentUserGymAsync();

        var dto = new MyGymDetailDto
        {
            Id = gym.Id,
            Name = gym.Name,
            Address = gym.Address,
            ImagePath = gym.ImagePath,
            AverageRating = gym.AverageRating,
            ReviewCount = gym.ReviewCount,
            CreatedAt = gym.CreatedAt,
            IsOpen = gym.IsOpenNow()
        };

        return ResponseModel<MyGymDetailDto>.SuccessResponse(dto);
    }

    public async Task<ResponseModel<bool>> UpdateMyGymAsync(GymUpdateDto dto)
    {
        var gym = await GetCurrentUserGymAsync();

        if (!string.IsNullOrWhiteSpace(dto.Name))
            gym.SetName(dto.Name);

        if (!string.IsNullOrWhiteSpace(dto.Address))
            gym.SetAddress(dto.Address);

        if (dto.Description is not null)
            gym.SetDescription(dto.Description);

        if (dto.Phone is not null || dto.Email is not null || dto.Website is not null)
            gym.SetContact(
                dto.Phone ?? gym.Phone,
                dto.Email ?? gym.Email,
                dto.Website ?? gym.Website);

        if (dto.Latitude.HasValue || dto.Longitude.HasValue)
            gym.SetLocation(dto.Latitude, dto.Longitude);

        _gymRepository.Update(gym);
        await _gymRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<MyGymStatsDto>> GetMyGymStatsAsync()
    {
        var gym = await GetCurrentUserGymAsync();

        var gymPackageIds = await _packageRepository.Table
            .Where(p => p.GymId == gym.Id && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync();

        var activeSubscriptions = await _subscriptionRepository.Table
            .Where(s => gymPackageIds.Contains(s.PackageId) && s.EndDate > DateTime.UtcNow)
            .CountAsync();

        var totalMembers = await _subscriptionRepository.Table
            .Where(s => gymPackageIds.Contains(s.PackageId))
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync();

        var today = DateTime.UtcNow.Date;
        var todayCheckIns = await _checkInLogRepository.Table
            .Where(c => c.CheckInTime.Date == today)
            .CountAsync();

        var totalPackages = gymPackageIds.Count;

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthlyRevenue = await _paymentRepository.Table
            .Where(p => gymPackageIds.Contains(p.PackageId) &&
                        p.Status == Domain.Enums.PaymentStatus.Successful &&
                        p.CreatedAt >= startOfMonth)
            .SumAsync(p => p.Amount);

        var stats = new MyGymStatsDto
        {
            TotalMembers = totalMembers,
            ActiveSubscriptions = activeSubscriptions,
            TodayCheckIns = todayCheckIns,
            TotalPackages = totalPackages,
            TotalReviews = gym.ReviewCount,
            AverageRating = gym.AverageRating,
            MonthlyRevenue = monthlyRevenue
        };

        return ResponseModel<MyGymStatsDto>.SuccessResponse(stats);
    }

    public async Task<ResponseModel<List<GymMemberDto>>> GetMyGymMembersAsync()
    {
        var gym = await GetCurrentUserGymAsync();

        var gymPackageIds = await _packageRepository.Table
            .Where(p => p.GymId == gym.Id && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync();

        var members = await _subscriptionRepository.Table
            .Include(s => s.User)
            .Include(s => s.Package)
            .Where(s => gymPackageIds.Contains(s.PackageId))
            .Select(s => new GymMemberDto
            {
                UserId = s.UserId,
                FullName = s.User.FullName,
                Email = s.User.Email!,
                PackageName = s.Package.Name,
                SubscriptionStartDate = s.StartDate,
                SubscriptionEndDate = s.EndDate,
                IsActive = s.EndDate > DateTime.UtcNow
            })
            .ToListAsync();

        return ResponseModel<List<GymMemberDto>>.SuccessResponse(members);
    }

    public async Task<ResponseModel<List<GymCheckInDto>>> GetTodayCheckInsAsync()
    {
        var gym = await GetCurrentUserGymAsync();

        var today = DateTime.UtcNow.Date;
        var checkIns = await _checkInLogRepository.Table
            .Include(c => c.User)
            .Where(c => c.CheckInTime.Date == today)
            .Select(c => new GymCheckInDto
            {
                UserId = c.UserId,
                UserFullName = c.User.FullName,
                CheckInTime = c.CheckInTime,
                CheckOutTime = c.CheckOutTime
            })
            .OrderByDescending(c => c.CheckInTime)
            .ToListAsync();

        return ResponseModel<List<GymCheckInDto>>.SuccessResponse(checkIns);
    }

    public async Task<ResponseModel<List<PackageResponseDto>>> GetMyPackagesAsync()
    {
        var gym = await GetCurrentUserGymAsync();

        var packages = await _packageRepository.Table
            .Where(p => p.GymId == gym.Id && !p.IsDeleted)
            .ToListAsync();

        var dtos = _mapper.Map<List<PackageResponseDto>>(packages);

        return ResponseModel<List<PackageResponseDto>>.SuccessResponse(dtos);
    }
}
