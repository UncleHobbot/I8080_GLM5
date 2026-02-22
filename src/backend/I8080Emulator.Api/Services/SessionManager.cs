using System;
using System.Collections.Concurrent;
using System.Linq;
using I8080Emulator.Api;

namespace I8080Emulator.Api.Services;

public class SessionManager
{
    private readonly ConcurrentDictionary<string, EmulatorSession> _sessions = new();
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30);
    
    public EmulatorSession CreateSession()
    {
        CleanupExpiredSessions();
        
        var session = new EmulatorSession();
        _sessions[session.Id] = session;
        
        session.Bdos.OnCharOutput += c => session.Output.Append(c);
        
        return session;
    }
    
    public EmulatorSession? GetSession(string id)
    {
        if (_sessions.TryGetValue(id, out var session))
        {
            session.LastAccess = DateTime.UtcNow;
            return session;
        }
        return null;
    }
    
    public bool RemoveSession(string id) => _sessions.TryRemove(id, out _);
    
    private void CleanupExpiredSessions()
    {
        var expired = _sessions
            .Where(x => DateTime.UtcNow - x.Value.LastAccess > _sessionTimeout)
            .Select(x => x.Key)
            .ToList();
        
        foreach (var id in expired)
            _sessions.TryRemove(id, out _);
    }
    
    public int ActiveSessionCount => _sessions.Count;
}
