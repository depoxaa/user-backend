using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class StreamViewerRepository : Repository<StreamViewer>, IStreamViewerRepository
{
    public StreamViewerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<int> GetActiveViewerCountAsync(Guid streamHostId)
    {
        return await _dbSet.CountAsync(sv =>
            sv.StreamHostId == streamHostId && sv.LeftAt == null);
    }

    public async Task<StreamViewer?> GetActiveViewerAsync(Guid streamHostId, Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(sv =>
            sv.StreamHostId == streamHostId &&
            sv.UserId == userId &&
            sv.LeftAt == null);
    }

    public async Task MarkViewerLeftAsync(Guid streamHostId, Guid userId)
    {
        var viewer = await GetActiveViewerAsync(streamHostId, userId);
        if (viewer != null)
        {
            viewer.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task CleanupStaleViewersAsync(TimeSpan timeout)
    {
        var cutoff = DateTime.UtcNow - timeout;
        var staleViewers = await _dbSet
            .Where(sv => sv.LeftAt == null && sv.JoinedAt < cutoff)
            .ToListAsync();

        foreach (var viewer in staleViewers)
        {
            viewer.LeftAt = DateTime.UtcNow;
        }

        if (staleViewers.Count > 0)
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllViewersLeftAsync(Guid streamHostId)
    {
        var activeViewers = await _dbSet
            .Where(sv => sv.StreamHostId == streamHostId && sv.LeftAt == null)
            .ToListAsync();

        foreach (var viewer in activeViewers)
        {
            viewer.LeftAt = DateTime.UtcNow;
        }

        if (activeViewers.Count > 0)
        {
            await _context.SaveChangesAsync();
        }
    }
}
