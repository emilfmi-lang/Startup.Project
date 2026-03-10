using Startup_Project.Application.Dtos.UserSession;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IUserSessionService
{
    /// <summary>
    /// "Book New" tab — bütün mövcud g?l?c?k sessiyalar (öd?ni? olunmam??).
    /// </summary>
    Task<ResponseModel<List<UserSessionDto>>> GetAvailableSessionsAsync(
        string? search = null,
        string? exerciseType = null,
        Guid? trainerId = null);

    /// <summary>
    /// "Upcoming" tab — user-?n öd?ni? edib qeyd? ald??? g?l?c?k sessiyalar.
    /// </summary>
    Task<ResponseModel<List<UserSessionDto>>> GetMyUpcomingSessionsAsync();

    /// <summary>
    /// "Past" tab — user-?n keçmi? sessiyalar? (tamamlanan + l??v olunan).
    /// </summary>
    Task<ResponseModel<List<UserSessionDto>>> GetMyPastSessionsAsync();

    /// <summary>
    /// Sessiya l??v etm? — user öz qeyd? ald??? sessiyan? l??v edir.
    /// </summary>
    Task<ResponseModel<bool>> CancelBookingAsync(Guid scheduledSessionId);
}
