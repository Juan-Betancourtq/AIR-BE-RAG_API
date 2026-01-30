using System.Collections.Concurrent;
using ResumeChat.RagApi.Models;

namespace ResumeChat.RagApi.Services;

public interface ISessionService
{
    ChatSession GetSession(string sessionId);
    void UpdateSession(ChatSession session);
    string CreateSession();
    void CleanupOldSessions();
}

public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, ChatSession> _sessions = new();
    private readonly ILogger<SessionService> _logger;
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(2);

    public SessionService(ILogger<SessionService> logger)
    {
        _logger = logger;
        
        // Start background cleanup task
        Task.Run(async () => await SessionCleanupLoop());
    }

    public ChatSession GetSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return session;
        }
        
        var newSession = new ChatSession { SessionId = sessionId };
        _sessions.TryAdd(sessionId, newSession);
        return newSession;
    }

    public void UpdateSession(ChatSession session)
    {
        _sessions.AddOrUpdate(session.SessionId, session, (key, existing) => session);
    }

    public string CreateSession()
    {
        var sessionId = Guid.NewGuid().ToString();
        var session = new ChatSession { SessionId = sessionId };
        _sessions.TryAdd(sessionId, session);
        
        _logger.LogInformation("Created new session: {SessionId}", sessionId);
        return sessionId;
    }

    public void CleanupOldSessions()
    {
        var cutoffTime = DateTime.UtcNow - _sessionTimeout;
        var expiredSessions = _sessions
            .Where(kvp => kvp.Value.LastActivity < cutoffTime)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var sessionId in expiredSessions)
        {
            _sessions.TryRemove(sessionId, out _);
            _logger.LogInformation("Removed expired session: {SessionId}", sessionId);
        }
    }

    private async Task SessionCleanupLoop()
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromMinutes(30));
            CleanupOldSessions();
        }
    }
}
