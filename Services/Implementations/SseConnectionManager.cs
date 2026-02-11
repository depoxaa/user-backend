using System.Collections.Concurrent;
using System.Text.Json;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class SseConnectionManager : ISseConnectionManager
{
    private readonly ConcurrentDictionary<Guid, SseConnection> _connections = new();
    private readonly ILogger<SseConnectionManager> _logger;

    public SseConnectionManager(ILogger<SseConnectionManager> logger)
    {
        _logger = logger;
    }

    public async Task RegisterConnectionAsync(Guid userId, HttpResponse response, CancellationToken cancellationToken)
    {
        // Remove existing connection for this user if any
        RemoveConnection(userId);

        var connection = new SseConnection(response, cancellationToken);
        _connections[userId] = connection;
        
        _logger.LogInformation("SSE connection registered for user {UserId}. Total connections: {Count}", 
            userId, _connections.Count);

        // Send initial heartbeat
        await SendEventAsync(userId, "connected", new { message = "SSE connection established" });

        // Keep connection alive with periodic heartbeats
        try
        {
            while (!cancellationToken.IsCancellationRequested && !connection.IsDisconnected)
            {
                await Task.Delay(30000, cancellationToken); // Heartbeat every 30 seconds
                if (!connection.IsDisconnected)
                {
                    await WriteToConnectionAsync(connection, "heartbeat", new { timestamp = DateTime.UtcNow });
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when client disconnects
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SSE connection error for user {UserId}", userId);
        }
        finally
        {
            RemoveConnection(userId);
        }
    }

    public void RemoveConnection(Guid userId)
    {
        if (_connections.TryRemove(userId, out var connection))
        {
            connection.Dispose();
            _logger.LogInformation("SSE connection removed for user {UserId}. Total connections: {Count}", 
                userId, _connections.Count);
        }
    }

    public async Task SendEventAsync(Guid userId, string eventType, object data)
    {
        if (_connections.TryGetValue(userId, out var connection) && !connection.IsDisconnected)
        {
            await WriteToConnectionAsync(connection, eventType, data);
        }
    }

    public async Task SendEventToUsersAsync(IEnumerable<Guid> userIds, string eventType, object data)
    {
        var tasks = userIds.Select(userId => SendEventAsync(userId, eventType, data));
        await Task.WhenAll(tasks);
    }

    public async Task BroadcastEventAsync(string eventType, object data)
    {
        var tasks = _connections.Keys.Select(userId => SendEventAsync(userId, eventType, data));
        await Task.WhenAll(tasks);
    }

    private async Task WriteToConnectionAsync(SseConnection connection, string eventType, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var message = $"event: {eventType}\ndata: {json}\n\n";
            await connection.WriteAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write to SSE connection");
            connection.MarkDisconnected();
        }
    }

    private class SseConnection : IDisposable
    {
        private readonly HttpResponse _response;
        private readonly CancellationToken _cancellationToken;
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        
        public bool IsDisconnected { get; private set; }

        public SseConnection(HttpResponse response, CancellationToken cancellationToken)
        {
            _response = response;
            _cancellationToken = cancellationToken;
        }

        public async Task WriteAsync(string message)
        {
            if (IsDisconnected || _cancellationToken.IsCancellationRequested)
                return;

            await _writeLock.WaitAsync(_cancellationToken);
            try
            {
                await _response.WriteAsync(message, _cancellationToken);
                await _response.Body.FlushAsync(_cancellationToken);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public void MarkDisconnected() => IsDisconnected = true;

        public void Dispose()
        {
            IsDisconnected = true;
            _writeLock.Dispose();
        }
    }
}
