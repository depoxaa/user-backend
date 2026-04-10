using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class StreamViewerService : IStreamViewerService
{
    private readonly IStreamViewerRepository _viewerRepository;
    private readonly IArtistRepository _artistRepository;
    private readonly IPlatformSettingService _settingService;

    public StreamViewerService(
        IStreamViewerRepository viewerRepository,
        IArtistRepository artistRepository,
        IPlatformSettingService settingService)
    {
        _viewerRepository = viewerRepository;
        _artistRepository = artistRepository;
        _settingService = settingService;
    }

    public async Task JoinStreamAsync(Guid streamHostId, Guid userId)
    {
        var artist = await _artistRepository.GetByIdAsync(streamHostId);
        if (artist == null || !artist.IsLive)
            throw new InvalidOperationException("Stream not found or not live");

        // Check if already viewing
        var existing = await _viewerRepository.GetActiveViewerAsync(streamHostId, userId);
        if (existing != null)
            return; // Already watching

        // Check viewer limit - artists are treated as ordinary hosts
        var hostRole = "User"; // Artists default to ordinary limits
        var limit = await _settingService.GetStreamViewerLimitAsync(hostRole);
        var currentCount = await _viewerRepository.GetActiveViewerCountAsync(streamHostId);

        if (limit != -1 && currentCount >= limit)
            throw new InvalidOperationException($"STREAM_FULL:{limit}");

        var viewer = new StreamViewer
        {
            StreamHostId = streamHostId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        await _viewerRepository.AddAsync(viewer);

        // Update listener count on artist
        artist.ListenersCount = await _viewerRepository.GetActiveViewerCountAsync(streamHostId);
        await _artistRepository.UpdateAsync(artist);
    }

    public async Task LeaveStreamAsync(Guid streamHostId, Guid userId)
    {
        await _viewerRepository.MarkViewerLeftAsync(streamHostId, userId);

        var artist = await _artistRepository.GetByIdAsync(streamHostId);
        if (artist != null)
        {
            artist.ListenersCount = await _viewerRepository.GetActiveViewerCountAsync(streamHostId);
            await _artistRepository.UpdateAsync(artist);
        }
    }

    public async Task<int> GetActiveViewerCountAsync(Guid streamHostId)
    {
        return await _viewerRepository.GetActiveViewerCountAsync(streamHostId);
    }

    public async Task EndStreamAsync(Guid streamHostId)
    {
        await _viewerRepository.MarkAllViewersLeftAsync(streamHostId);
    }
}
