namespace Startup_Project.Application.Abstractions.Chat;

public interface IPresenceTracker
{
    Task<bool> UserConnected(Guid userId, string connectionId);
    Task<bool> UserDisconnected(Guid userId, string connectionId);
    Task<bool> IsUserOnline(Guid userId);
    Task<List<Guid>> GetOnlineUsers();
    Task<List<string>> GetConnectionsForUser(Guid userId);
}
