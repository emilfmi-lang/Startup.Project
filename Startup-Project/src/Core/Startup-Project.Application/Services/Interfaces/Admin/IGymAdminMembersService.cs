using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Dtos.Common;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Admin;

/// <summary>
/// Gym Owner/Admin Panel - Members Management Service
/// GymOwner multiple gyms (branches) own ed? bil?c?yi üçün
/// members filtering gym-a ?sasl? olur
/// Senior-level: Repository pattern, AppUser.GymId dependency removed
/// </summary>
public interface IGymAdminMembersService
{
    /// <summary>
    /// GymOwner-? m?xsus gyms-l?rin siyah?s?n? getir
    /// Owner-?n Gym.OwnerId == ownerId olan bütün gym-l?ri
    /// </summary>
    /// <param name="ownerId">GymOwner user ID-si</param>
    /// <returns>Owned gyms list with member/trainer counts</returns>
    Task<ResponseModel<List<GymBasicInfoDto>>> GetOwnedGymsAsync(Guid ownerId);

    /// <summary>
    /// Members siyah?s?n? getir (single gym yaxud all gyms)
    /// GymOwner yaln?z öz gym-l?rin? giri? ed? bil?r
    /// .ProjectTo() il? SQL-d? DTO conversion (performance optimized)
    /// </summary>
    /// <param name="ownerId">GymOwner user ID-si</param>
    /// <param name="filter">Filter parametrl?ri (SelectedGymId il? gym seç? bil?c?yi)</param>
    /// <returns>Paginated members list</returns>
    Task<ResponseModel<PaginatedResult<MemberListItemDto>>> GetMembersAsync(
        Guid ownerId,
        AdminMembersFilterDto filter);

    /// <summary>
    /// Member detail m?lumat? getir
    /// </summary>
    /// <param name="ownerId">GymOwner user ID-si</param>
    /// <param name="memberId">Member user ID-si</param>
    /// <returns>Member details</returns>
    Task<ResponseModel<MemberDetailDto>> GetMemberDetailAsync(Guid ownerId, Guid memberId);

    /// <summary>
    /// Member-i trainer-? çevir
    /// Yeni JWT token döndürür (trainer role il?)
    /// </summary>
    /// <param name="ownerId">GymOwner user ID-si</param>
    /// <param name="memberId">Member user ID-si</param>
    /// <param name="gymId">Hans? gym-d? trainer olacaq</param>
    /// <param name="request">Trainer m?lumatlar?</param>
    /// <returns>Trainer info + new JWT token</returns>
    Task<ResponseModel<MakeTrainerResponseDto>> MakeMemberTrainerAsync(
        Guid ownerId,
        Guid memberId,
        Guid gymId,
        MakeTrainerRequestDto request);

    /// <summary>
    /// Trainers siyah?s?n? getir
    /// </summary>
    /// <param name="ownerId">GymOwner user ID-si</param>
    /// <param name="gymId">Hans? gym-? aid trainers</param>
    /// <param name="page">S?hif? nömr?si</param>
    /// <param name="pageSize">S?hif? ölçüsü</param>
    /// <returns>Trainers list</returns>
    Task<ResponseModel<PaginatedResult<TrainerListItemDto>>> GetTrainersAsync(
        Guid ownerId,
        Guid gymId,
        int page = 1,
        int pageSize = 10);

    /// <summary>
    /// Trainer detail m?lumat? getir
    /// </summary>
    /// <param name="ownerId">GymOwner user ID-si</param>
    /// <param name="trainerId">Trainer ID-si</param>
    /// <returns>Trainer details</returns>
    Task<ResponseModel<TrainerDetailDto>> GetTrainerDetailAsync(Guid ownerId, Guid trainerId);
}

/// <summary>
/// GymOwner-?n owned gym-l?ri üçün basic info
/// </summary>
public class GymBasicInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int MemberCount { get; set; }
    public int TrainerCount { get; set; }
}

/// <summary>
/// Trainer detail info
/// </summary>
public class TrainerDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public GymInfoDto? Gym { get; set; }
    public string Specialization { get; set; } = null!;
    public int Experience { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public TrainerStatsDto? Stats { get; set; }
}

/// <summary>
/// Trainer statistics
/// </summary>
public class TrainerStatsDto
{
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int TotalClients { get; set; }
    public double Rating { get; set; }
    public int TotalReviews { get; set; }
    public DateTime JoinDate { get; set; }
}
