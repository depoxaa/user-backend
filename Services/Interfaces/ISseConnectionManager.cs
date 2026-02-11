namespace backend.Services.Interfaces;

public interface ISseConnectionManager
{
    /// <summary>
    /// Register a user's SSE connection
    /// </summary>
    Task RegisterConnectionAsync(Guid userId, HttpResponse response, CancellationToken cancellationToken);
    
    /// <summary>
    /// Remove a user's connection when they disconnect
    /// </summary>
    void RemoveConnection(Guid userId);
    
    /// <summary>
    /// Send an event to a specific user
    /// </summary>
    Task SendEventAsync(Guid userId, string eventType, object data);
    
    /// <summary>
    /// Send an event to multiple users
    /// </summary>
    Task SendEventToUsersAsync(IEnumerable<Guid> userIds, string eventType, object data);
    
    /// <summary>
    /// Broadcast an event to all connected users
    /// </summary>
    Task BroadcastEventAsync(string eventType, object data);
}
