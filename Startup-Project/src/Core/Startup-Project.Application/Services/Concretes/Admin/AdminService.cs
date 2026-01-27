using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Admin;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Services.Concretes.Admin;

public class AdminService : IAdminService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IGenericRepository<Gym> _gymRepository;
    private readonly IGenericRepository<Subscription> _subscriptionRepository;
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IGenericRepository<CheckInLog> _checkInLogRepository;
    private readonly IMapper _mapper;

    public AdminService(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IGenericRepository<Gym> gymRepository,
        IGenericRepository<Subscription> subscriptionRepository,
        IGenericRepository<Payment> paymentRepository,
        IGenericRepository<CheckInLog> checkInLogRepository,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _gymRepository = gymRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _checkInLogRepository = checkInLogRepository;
        _mapper = mapper;
    }

    public async Task<ResponseModel<DashboardStatsDto>> GetDashboardStatsAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalGyms = await _gymRepository.Table.Where(g => !g.IsDeleted).CountAsync();
        var totalActiveSubscriptions = await _subscriptionRepository.Table
            .Where(s => s.EndDate > DateTime.UtcNow)
            .CountAsync();

        var totalRevenue = await _paymentRepository.Table
            .Where(p => p.Status == PaymentStatus.Successful)
            .SumAsync(p => p.Amount);

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthlyRevenue = await _paymentRepository.Table
            .Where(p => p.Status == PaymentStatus.Successful && p.CreatedAt >= startOfMonth)
            .SumAsync(p => p.Amount);

        var newUsersThisMonth = await _userManager.Users
            .CountAsync();

        var today = DateTime.UtcNow.Date;
        var todayCheckIns = await _checkInLogRepository.Table
            .Where(c => c.CheckInTime.Date == today)
            .CountAsync();

        var stats = new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalGyms = totalGyms,
            TotalActiveSubscriptions = totalActiveSubscriptions,
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue,
            NewUsersThisMonth = newUsersThisMonth,
            TodayCheckIns = todayCheckIns
        };

        return ResponseModel<DashboardStatsDto>.SuccessResponse(stats);
    }

    public async Task<ResponseModel<List<UserListDto>>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserListDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserListDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = roles.ToList(),
                EmailConfirmed = user.EmailConfirmed,
                IsBlocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow
            });
        }

        return ResponseModel<List<UserListDto>>.SuccessResponse(userDtos);
    }

    public async Task<ResponseModel<bool>> AssignRoleAsync(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
            return ResponseModel<bool>.FailureResponse("Istifadeci tapilmadi.");

        if (!await _roleManager.RoleExistsAsync(dto.RoleName))
            return ResponseModel<bool>.FailureResponse("Bu rol movcud deyil.");

        if (await _userManager.IsInRoleAsync(user, dto.RoleName))
            return ResponseModel<bool>.FailureResponse("Istifadeci artiq bu roldadir.");

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
            return ResponseModel<bool>.FailureResponse("Rol teyin edilemedi.");

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> RemoveRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ResponseModel<bool>.FailureResponse("Istifadeci tapilmadi.");

        if (!await _userManager.IsInRoleAsync(user, roleName))
            return ResponseModel<bool>.FailureResponse("Istifadeci bu rolda deyil.");

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
            return ResponseModel<bool>.FailureResponse("Rol siline bilmedi.");

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> BlockUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ResponseModel<bool>.FailureResponse("Istifadeci tapilmadi.");

        user.LockoutEnd = DateTimeOffset.MaxValue;
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
            return ResponseModel<bool>.FailureResponse("Istifadeci bloklana bilmedi.");

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UnblockUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ResponseModel<bool>.FailureResponse("Istifadeci tapilmadi.");

        user.LockoutEnd = null;
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
            return ResponseModel<bool>.FailureResponse("Istifadecinin bloku acila bilmedi.");

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> AssignGymOwnerAsync(AssignGymOwnerDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
            return ResponseModel<bool>.FailureResponse("Istifadeci tapilmadi.");

        var gym = await _gymRepository.GetByIdAsync(dto.GymId);
        if (gym == null || gym.IsDeleted)
            return ResponseModel<bool>.FailureResponse("Zal tapilmadi.");

        if (!await _userManager.IsInRoleAsync(user, "GymOwner"))
        {
            await _userManager.AddToRoleAsync(user, "GymOwner");
        }

        gym.SetOwner(dto.UserId);
        _gymRepository.Update(gym);
        await _gymRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<List<GymGetDto>>> GetAllGymsAsync()
    {
        var gyms = await _gymRepository.Table
            .Include(g => g.Owner)
            .Where(g => !g.IsDeleted)
            .ToListAsync();

        var dtos = _mapper.Map<List<GymGetDto>>(gyms);
        return ResponseModel<List<GymGetDto>>.SuccessResponse(dtos);
    }
}
