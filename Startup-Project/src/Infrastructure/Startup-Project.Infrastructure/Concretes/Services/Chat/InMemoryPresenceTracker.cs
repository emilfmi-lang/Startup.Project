using System.Collections.Concurrent;
using Startup_Project.Application.Abstractions.Chat;

namespace Startup_Project.Infrastructure.Concretes.Services.Chat;

public class InMemoryPresenceTracker : IPresenceTracker
{
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _onlineUsers = new();
    private static readonly object _lock = new();

    public Task<bool> UserConnected(Guid userId, string connectionId)
    {
        bool isFirstConnection = false;

        lock (_lock)
        {
            if (!_onlineUsers.ContainsKey(userId))
            {
                _onlineUsers[userId] = [];
                isFirstConnection = true;
            }
            _onlineUsers[userId].Add(connectionId);
        }

        return Task.FromResult(isFirstConnection);
    }

    public Task<bool> UserDisconnected(Guid userId, string connectionId)
    {
        bool isLastConnection = false;

        lock (_lock)
        {
            if (_onlineUsers.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _onlineUsers.TryRemove(userId, out _);
                    isLastConnection = true;
                }
            }
        }

        return Task.FromResult(isLastConnection);
    }

    public Task<bool> IsUserOnline(Guid userId)
    {
        return Task.FromResult(_onlineUsers.ContainsKey(userId));
    }

    public Task<List<Guid>> GetOnlineUsers()
    {
        return Task.FromResult(_onlineUsers.Keys.ToList());
    }

    public Task<List<string>> GetConnectionsForUser(Guid userId)
    {
        lock (_lock)
        {
            if (_onlineUsers.TryGetValue(userId, out var connections))
                return Task.FromResult(connections.ToList());
        }
        return Task.FromResult(new List<string>());
    }
}
