using AutoMapper;
using FluentValidation;
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
using Startup_Project.Domain.Exceptions;
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
    private readonly IValidator<TrainerCreateDto> _createValidator;
    private readonly IValidator<TrainerUpdateDto> _updateValidator;
    private readonly IValidator<OnlineTrainerApplyDto> _applyValidator;
    private readonly IValidator<TrainerVerifyDto> _verifyValidator;

    public TrainerService(
        IGenericRepository<Trainer> trainerRepository,
        IGenericRepository<TrainerReview> reviewRepository,
        IGenericRepository<TrainerSchedule> scheduleRepository,
        IGenericRepository<Gym> gymRepository,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        IValidator<TrainerCreateDto> createValidator,
        IValidator<TrainerUpdateDto> updateValidator,
        IValidator<OnlineTrainerApplyDto> applyValidator,
        IValidator<TrainerVerifyDto> verifyValidator)
    {
        _trainerRepository = trainerRepository;
        _reviewRepository = reviewRepository;
        _scheduleRepository = scheduleRepository;
        _gymRepository = gymRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _applyValidator = applyValidator;
        _verifyValidator = verifyValidator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw BusinessException.UnauthorizedAccess();
        return userId;
    }

    private async Task<Guid> GetCurrentUserGymIdAsync()
    {
        var userId = GetCurrentUserId();
        var gym = await _gymRepository.Table
            .FirstOrDefaultAsync(g => g.OwnerId == userId && !g.IsDeleted);

        if (gym == null)
            throw NotFoundException.GymForOwner();

        return gym.Id;
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

        var dtos = _mapper.Map<List<TrainerListDto>>(trainers);
        return ResponseModel<List<TrainerListDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<TrainerDto>> GetTrainerByIdAsync(Guid trainerId)
    {
        var trainer = await _trainerRepository.Table
            .Include(t => t.User)
            .Include(t => t.Gym)
            .FirstOrDefaultAsync(t => t.Id == trainerId && !t.IsDeleted);

        if (trainer == null)
            throw NotFoundException.Trainer(trainerId);

        var dto = _mapper.Map<TrainerDto>(trainer);
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

        var dtos = _mapper.Map<List<TrainerListDto>>(trainers);
        return ResponseModel<List<TrainerListDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<List<TrainerListDto>>> GetOnlineTrainersAsync()
    {
        var trainers = await _trainerRepository.Table
            .Include(t => t.User)
            .Where(t => t.IsOnlineTrainer && t.IsVerified && t.Status == TrainerStatus.Active && !t.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<TrainerListDto>>(trainers);
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

        var dtos = _mapper.Map<List<TrainerReviewDto>>(reviews);
        return ResponseModel<List<TrainerReviewDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<List<TrainerScheduleDto>>> GetTrainerScheduleAsync(Guid trainerId)
    {
        var schedules = await _scheduleRepository.Table
            .Where(s => s.TrainerId == trainerId && !s.IsDeleted)
            .OrderBy(s => s.DayOfWeek)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<TrainerScheduleDto>>(schedules);
        return ResponseModel<List<TrainerScheduleDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<bool>> AddReviewAsync(Guid trainerId, TrainerReviewCreateDto dto)
    {
        var userId = GetCurrentUserId();

        var trainer = await _trainerRepository.GetByIdAsync(trainerId);
        if (trainer == null || trainer.IsDeleted)
            throw NotFoundException.Trainer(trainerId);

        var existingReview = await _reviewRepository.Table
            .FirstOrDefaultAsync(r => r.TrainerId == trainerId && r.UserId == userId);
        if (existingReview != null)
            throw BusinessException.AlreadyReviewed();

        if (dto.StarCount < 1 || dto.StarCount > 5)
            throw BusinessException.ValidationFailed("Ulduz 1-5 arası olmalıdır.");

        var review = new TrainerReview(trainerId, userId, dto.StarCount, dto.Comment);
        await _reviewRepository.AddAsync(review);

        trainer.AddReview(dto.StarCount);
        _trainerRepository.Update(trainer);

        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    // ══════════════════════════════════════════════════════════════
    // ONLINE TRAINER APPLICATION
    // ══════════════════════════════════════════════════════════════

    public async Task<ResponseModel<bool>> ApplyAsOnlineTrainerAsync(OnlineTrainerApplyDto dto)
    {
        var validationResult = await _applyValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var userId = GetCurrentUserId();

        var existingTrainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);
        if (existingTrainer != null)
            throw BusinessException.TrainerAlreadyExists();

        var trainer = new Trainer(userId, dto.Specialization, dto.SessionPrice);

        if (!string.IsNullOrWhiteSpace(dto.Bio))
            trainer.SetBio(dto.Bio);
        if (!string.IsNullOrWhiteSpace(dto.CertificateUrl))
            trainer.SetCertificateUrl(dto.CertificateUrl);
        if (!string.IsNullOrWhiteSpace(dto.VideoIntroUrl))
            trainer.SetVideoIntroUrl(dto.VideoIntroUrl);
        if (!string.IsNullOrWhiteSpace(dto.SocialMediaLinks))
            trainer.SetSocialMediaLinks(dto.SocialMediaLinks);

        await _trainerRepository.AddAsync(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    // ══════════════════════════════════════════════════════════════
    // GYM OWNER ENDPOINTS
    // ══════════════════════════════════════════════════════════════

    public async Task<ResponseModel<bool>> CreateTrainerAsync(TrainerCreateDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var gymId = await GetCurrentUserGymIdAsync();

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
            throw NotFoundException.User(dto.UserId);

        var existingTrainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.UserId == dto.UserId && !t.IsDeleted);
        if (existingTrainer != null)
            throw BusinessException.TrainerAlreadyExists();

        var trainer = new Trainer(dto.UserId, gymId, dto.Specialization, dto.SessionPrice);
        if (!string.IsNullOrWhiteSpace(dto.Bio))
            trainer.SetBio(dto.Bio);

        await _trainerRepository.AddAsync(trainer);

        if (!await _userManager.IsInRoleAsync(user, "Trainer"))
            await _userManager.AddToRoleAsync(user, "Trainer");

        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UpdateTrainerAsync(Guid trainerId, TrainerUpdateDto dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var gymId = await GetCurrentUserGymIdAsync();

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);
        if (trainer == null)
            throw NotFoundException.Trainer(trainerId);

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

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);
        if (trainer == null)
            throw NotFoundException.Trainer(trainerId);

        trainer.IsDeleted = true;
        _trainerRepository.Update(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> ActivateTrainerAsync(Guid trainerId)
    {
        var gymId = await GetCurrentUserGymIdAsync();

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);
        if (trainer == null)
            throw NotFoundException.Trainer(trainerId);

        trainer.Activate();
        _trainerRepository.Update(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeactivateTrainerAsync(Guid trainerId)
    {
        var gymId = await GetCurrentUserGymIdAsync();

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);
        if (trainer == null)
            throw NotFoundException.Trainer(trainerId);

        trainer.Deactivate();
        _trainerRepository.Update(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> SetTrainerScheduleAsync(Guid trainerId, List<TrainerScheduleDto> schedules)
    {
        var gymId = await GetCurrentUserGymIdAsync();

        var trainer = await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.GymId == gymId && !t.IsDeleted);
        if (trainer == null)
            throw NotFoundException.Trainer(trainerId);

        var existingSchedules = await _scheduleRepository.GetWhereAsync(s => s.TrainerId == trainerId);
        foreach (var s in existingSchedules)
            _scheduleRepository.Remove(s);

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
    // ADMIN ENDPOINTS
    // ══════════════════════════════════════════════════════════════

    public async Task<ResponseModel<List<TrainerListDto>>> GetPendingOnlineTrainersAsync()
    {
        var trainers = await _trainerRepository.Table
            .Include(t => t.User)
            .Where(t => t.IsOnlineTrainer && !t.IsVerified && t.Status == TrainerStatus.Pending && !t.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<TrainerListDto>>(trainers);
        return ResponseModel<List<TrainerListDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<bool>> VerifyOnlineTrainerAsync(Guid trainerId, TrainerVerifyDto dto)
    {
        var validationResult = await _verifyValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var trainer = await _trainerRepository.Table
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == trainerId && t.IsOnlineTrainer && !t.IsDeleted);
        if (trainer == null)
            throw NotFoundException.Trainer(trainerId);

        if (trainer.IsVerified)
            throw BusinessException.Custom(BusinessErrorCodes.ValidationFailed, "Bu məşqçi artıq təsdiqlənib.");

        if (dto.Approve)
        {
            trainer.Verify();

            var user = trainer.User;
            if (!await _userManager.IsInRoleAsync(user, "Trainer"))
                await _userManager.AddToRoleAsync(user, "Trainer");
        }
        else
        {
            trainer.Reject();
        }

        _trainerRepository.Update(trainer);
        await _trainerRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }


    public async Task<ResponseModel<TrainerDto>> GetMyProfileAsync()
    {
            var userId = GetCurrentUserId();

            var trainer = await _trainerRepository.Table
                .Include(t => t.User)
                .Include(t => t.Gym)
                .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);

            if (trainer == null)
                throw NotFoundException.Trainer(userId);

            var dto = _mapper.Map<TrainerDto>(trainer);
            return ResponseModel<TrainerDto>.SuccessResponse(dto);
        }

        public async Task<ResponseModel<bool>> UpdateMyProfileAsync(TrainerUpdateDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var userId = GetCurrentUserId();

            var trainer = await _trainerRepository.Table
                .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);

            if (trainer == null)
                throw NotFoundException.Trainer(userId);

            if (!string.IsNullOrWhiteSpace(dto.Specialization))
                trainer.SetSpecialization(dto.Specialization);

            if (dto.Bio != null)
                trainer.SetBio(dto.Bio);

            if (!string.IsNullOrWhiteSpace(dto.VideoIntroUrl))
                trainer.SetVideoIntroUrl(dto.VideoIntroUrl);

            if (!string.IsNullOrWhiteSpace(dto.SocialMediaLinks))
                trainer.SetSocialMediaLinks(dto.SocialMediaLinks);

            if (dto.SessionPrice.HasValue && trainer.IsOnlineTrainer)
                trainer.SetSessionPrice(dto.SessionPrice.Value);

            _trainerRepository.Update(trainer);
            await _trainerRepository.SaveAsync();

            return ResponseModel<bool>.SuccessResponse(true);
        }

        public async Task<ResponseModel<bool>> UpdateMyScheduleAsync(List<TrainerScheduleDto> schedules)
        {
            var userId = GetCurrentUserId();

            var trainer = await _trainerRepository.Table
                .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);

            if (trainer == null)
                throw NotFoundException.Trainer(userId);

            var existingSchedules = await _scheduleRepository.GetWhereAsync(s => s.TrainerId == trainer.Id);
            foreach (var s in existingSchedules)
                _scheduleRepository.Remove(s);

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
