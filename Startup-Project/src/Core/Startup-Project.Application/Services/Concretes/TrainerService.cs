using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Dtos.TrainerReview;
using Startup_Project.Application.Dtos.TrainerSchedule;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class TrainerService : ITrainerService
{
    private readonly IGenericRepository<Trainer> _trainerRepository;
    private readonly IGenericRepository<TrainerReview> _reviewRepository;
    private readonly IGenericRepository<TrainerSchedule> _scheduleRepository;
    private readonly IGenericRepository<Gym> _gymRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public TrainerService(
        IGenericRepository<Trainer> trainerRepository,
        IGenericRepository<TrainerReview> reviewRepository,
        IGenericRepository<TrainerSchedule> scheduleRepository,
        IGenericRepository<Gym> gymRepository,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _trainerRepository = trainerRepository;
        _reviewRepository = reviewRepository;
        _scheduleRepository = scheduleRepository;
        _gymRepository = gymRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;
        return userId;
    }

    private async Task<Guid?> GetCurrentUserGymIdAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return null;

        var gym = await _gymRepository.Table
            .FirstOrDefaultAsync(g => g.OwnerId == userId && !g.IsDeleted);
        return gym?.Id;
    }

    // ══════════════════════════════════════════════════════════════
    // PUBLIC ENDPOINTS
    // ══════════════════════════════════════════════════════════════

    public async Task<ResponseModel<List<TrainerListDto>>> GetAllTrainersAsync()
    {
        var trainers = await _trainerRepository.Table
            .Include(t => t.User)
            .Include(t => t.Gym)
            .Where(t => t.Status == TrainerStatus.Active && !t.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var dtos = trainers.Select(t => new TrainerListDto
        {
            Id = t.Id,
            UserFullName = t.User.FullName,
            Specialization = t.Specialization,
            ProfileImageUrl = t.ProfileImageUrl,
            SessionPrice = t.SessionPrice,
            AverageRating = t.AverageRating,
            ReviewCount = t.ReviewCount,
            GymName = t.Gym.Name
        }).ToList();

        return ResponseModel<List<TrainerListDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<TrainerDto>> GetTrainerByIdAsync(Guid trainerId)
    {
        var trainer = await _trainerRepository.Table
            .Include(t => t.User)
            .Include(t => t.Gym)
            .FirstOrDefaultAsync(t => t.Id == trainerId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<TrainerDto>.FailureResponse("Mesqci tapilmadi.");

        var dto = new TrainerDto
        {
            Id = trainer.Id,
            UserId = trainer.UserId,
            UserFullName = trainer.User.FullName,
            GymId = trainer.GymId,
            GymName = trainer.Gym.Name,
            Specialization = trainer.Specialization,
            Bio = trainer.Bio,
            CertificateUrl = trainer.CertificateUrl,
            ProfileImageUrl = trainer.ProfileImageUrl,
            SessionPrice = trainer.SessionPrice,
            AverageRating = trainer.AverageRating,
            ReviewCount = trainer.ReviewCount,
            TotalClients = trainer.TotalClients,
            TotalSessions = trainer.TotalSessions,
            Status = trainer.Status.ToString()
        };

        return ResponseModel<TrainerDto>.SuccessResponse(dto);
    }

    public async Task<ResponseModel<List<TrainerListDto>>> GetTrainersByGymAsync(Guid gymId)
    {
        var trainers = await _trainerRepository.Table
            .Include(t => t.User)
            .Include(t => t.Gym)
            .Where(t => t.GymId == gymId && t.Status == TrainerStatus.Active && !t.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var dtos = trainers.Select(t => new TrainerListDto
        {
            Id = t.Id,
            UserFullName = t.User.FullName,
            Specialization = t.Specialization,
            ProfileImageUrl = t.ProfileImageUrl,
            SessionPrice = t.SessionPrice,
            AverageRating = t.AverageRating,
            ReviewCount = t.ReviewCount,
            GymName = t.Gym.Name
        }).ToList();

        return ResponseModel<List<TrainerListDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<List<TrainerReviewDto>>> GetTrainerReviewsAsync(Guid trainerId)
    {
        var reviews = await _reviewRepository.Table
            .Include(r => r.User)
            .Where(r => r.TrainerId == trainerId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var dtos = reviews.Select(r => new TrainerReviewDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserFullName = r.User.FullName,
            StarCount = r.StarCount,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList();

        return ResponseModel<List<TrainerReviewDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<List<TrainerScheduleDto>>> GetTrainerScheduleAsync(Guid trainerId)
    {
        var schedules = await _scheduleRepository.Table
            .Where(s => s.TrainerId == trainerId && !s.IsDeleted)
            .OrderBy(s => s.DayOfWeek)
            .AsNoTracking()
            .ToListAsync();

        var dtos = schedules.Select(s => new TrainerScheduleDto
        {
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime.ToString(@"hh\:mm"),
            EndTime = s.EndTime.ToString(@"hh\:mm"),
            IsAvailable = s.IsAvailable
        }).ToList();

        return ResponseModel<List<TrainerScheduleDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<bool>> AddReviewAsync(Guid trainerId, TrainerReviewCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<bool>.FailureResponse("Giris etmelisiniz.");

        var trainer = await _trainerRepository.GetByIdAsync(trainerId);
        if (trainer == null || trainer.IsDeleted)
            return ResponseModel<bool>.FailureResponse("Mesqci tapilmadi.");

        var existingReview = await _reviewRepository.Table
            .FirstOrDefaultAsync(r => r.TrainerId == trainerId && r.UserId == userId);

        if (existingReview != null)
            return ResponseModel<bool>.FailureResponse("Bu mesqciye artiq review yazmisiz.");

        if (dto.StarCount < 1 || dto.StarCount > 5)
            return ResponseModel<bool>.FailureResponse("Ulduz 1-5 arasi olmalidir.");

        var review = new TrainerReview(trainerId, userId.Value, dto.StarCount, dto.Comment);
        await _reviewRepository.AddAsync(review);

        trainer.AddReview(dto.StarCount);
        _trainerRepository.Update(trainer);

        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    // ══════════════════════════════════════════════════════════════
    // GYM OWNER ENDPOINTS
    // ══════════════════════════════════════════════════════════════

    public async Task<ResponseModel<bool>> CreateTrainerAsync(TrainerCreateDto dto)
    {
        var gymId = await GetCurrentUserGymIdAsync();
        if (gymId == null)
            return ResponseModel<bool>.FailureResponse("Zaliniz tapilmadi.");

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
            return ResponseModel<bool>.FailureResponse("Istifadeci tapilmadi.");

        var existingTrainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.UserId == dto.UserId && !t.IsDeleted);

        if (existingTrainer != null)
            return ResponseModel<bool>.FailureResponse("Bu istifadeci artiq mesqcidir.");

        var trainer = new Trainer(dto.UserId, gymId.Value, dto.Specialization, dto.SessionPrice);
        if (!string.IsNullOrWhiteSpace(dto.Bio))
            trainer.SetBio(dto.Bio);

        await _trainerRepository.AddAsync(trainer);

        // User-e Trainer rolu ver
        if (!await _userManager.IsInRoleAsync(user, "Trainer"))
        {
            await _userManager.AddToRoleAsync(user, "Trainer");
        }

        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UpdateTrainerAsync(Guid trainerId, TrainerUpdateDto dto)
    {
        var gymId = await GetCurrentUserGymIdAsync();
        if (gymId == null)
            return ResponseModel<bool>.FailureResponse("Zaliniz tapilmadi.");

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<bool>.FailureResponse("Mesqci tapilmadi ve ya sizin zaliniza aid deyil.");

        if (!string.IsNullOrWhiteSpace(dto.Specialization))
            trainer.SetSpecialization(dto.Specialization);

        if (dto.Bio != null)
            trainer.SetBio(dto.Bio);

        if (dto.SessionPrice.HasValue)
            trainer.SetSessionPrice(dto.SessionPrice.Value);

        _trainerRepository.Update(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeleteTrainerAsync(Guid trainerId)
    {
        var gymId = await GetCurrentUserGymIdAsync();
        if (gymId == null)
            return ResponseModel<bool>.FailureResponse("Zaliniz tapilmadi.");

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<bool>.FailureResponse("Mesqci tapilmadi ve ya sizin zaliniza aid deyil.");

        trainer.IsDeleted = true;
        _trainerRepository.Update(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> ActivateTrainerAsync(Guid trainerId)
    {
        var gymId = await GetCurrentUserGymIdAsync();
        if (gymId == null)
            return ResponseModel<bool>.FailureResponse("Zaliniz tapilmadi.");

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<bool>.FailureResponse("Mesqci tapilmadi ve ya sizin zaliniza aid deyil.");

        trainer.Activate();
        _trainerRepository.Update(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeactivateTrainerAsync(Guid trainerId)
    {
        var gymId = await GetCurrentUserGymIdAsync();
        if (gymId == null)
            return ResponseModel<bool>.FailureResponse("Zaliniz tapilmadi.");

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<bool>.FailureResponse("Mesqci tapilmadi ve ya sizin zaliniza aid deyil.");

        trainer.Deactivate();
        _trainerRepository.Update(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> SetTrainerScheduleAsync(Guid trainerId, List<TrainerScheduleDto> schedules)
    {
        var gymId = await GetCurrentUserGymIdAsync();
        if (gymId == null)
            return ResponseModel<bool>.FailureResponse("Zaliniz tapilmadi.");

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<bool>.FailureResponse("Mesqci tapilmadi ve ya sizin zaliniza aid deyil.");

        // Kohne schedule-lari sil
        var existingSchedules = await _scheduleRepository.GetWhereAsync(s => s.TrainerId == trainerId);
        foreach (var s in existingSchedules)
        {
            _scheduleRepository.Remove(s);
        }

        // Yeni schedule-lari elave et
        foreach (var dto in schedules)
        {
            if (!TimeSpan.TryParse(dto.StartTime, out var startTime) ||
                !TimeSpan.TryParse(dto.EndTime, out var endTime))
                continue;

            var schedule = new TrainerSchedule(trainerId, dto.DayOfWeek, startTime, endTime);
            await _scheduleRepository.AddAsync(schedule);
        }

        await _scheduleRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    // ══════════════════════════════════════════════════════════════
    // TRAINER OWN PROFILE
    // ══════════════════════════════════════════════════════════════

    public async Task<ResponseModel<TrainerDto>> GetMyProfileAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<TrainerDto>.FailureResponse("Giris etmelisiniz.");

        var trainer = await _trainerRepository.Table
            .Include(t => t.User)
            .Include(t => t.Gym)
            .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<TrainerDto>.FailureResponse("Mesqci profiliniz tapilmadi.");

        var dto = new TrainerDto
        {
            Id = trainer.Id,
            UserId = trainer.UserId,
            UserFullName = trainer.User.FullName,
            GymId = trainer.GymId,
            GymName = trainer.Gym.Name,
            Specialization = trainer.Specialization,
            Bio = trainer.Bio,
            CertificateUrl = trainer.CertificateUrl,
            ProfileImageUrl = trainer.ProfileImageUrl,
            SessionPrice = trainer.SessionPrice,
            AverageRating = trainer.AverageRating,
            ReviewCount = trainer.ReviewCount,
            TotalClients = trainer.TotalClients,
            TotalSessions = trainer.TotalSessions,
            Status = trainer.Status.ToString()
        };

        return ResponseModel<TrainerDto>.SuccessResponse(dto);
    }

    public async Task<ResponseModel<bool>> UpdateMyProfileAsync(TrainerUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<bool>.FailureResponse("Giris etmelisiniz.");

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<bool>.FailureResponse("Mesqci profiliniz tapilmadi.");

        if (!string.IsNullOrWhiteSpace(dto.Specialization))
            trainer.SetSpecialization(dto.Specialization);

        if (dto.Bio != null)
            trainer.SetBio(dto.Bio);

        // Trainer oz qiymetini deyise bilmez - yalniz GymOwner
        // if (dto.SessionPrice.HasValue)
        //     trainer.SetSessionPrice(dto.SessionPrice.Value);

        _trainerRepository.Update(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UpdateMyScheduleAsync(List<TrainerScheduleDto> schedules)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<bool>.FailureResponse("Giris etmelisiniz.");

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<bool>.FailureResponse("Mesqci profiliniz tapilmadi.");

        // Kohne schedule-lari sil
        var existingSchedules = await _scheduleRepository.GetWhereAsync(s => s.TrainerId == trainer.Id);
        foreach (var s in existingSchedules)
        {
            _scheduleRepository.Remove(s);
        }

        // Yeni schedule-lari elave et
        foreach (var dto in schedules)
        {
            if (!TimeSpan.TryParse(dto.StartTime, out var startTime) ||
                !TimeSpan.TryParse(dto.EndTime, out var endTime))
                continue;

            var schedule = new TrainerSchedule(trainer.Id, dto.DayOfWeek, startTime, endTime);
            await _scheduleRepository.AddAsync(schedule);
        }

        await _scheduleRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
