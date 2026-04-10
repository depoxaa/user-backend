using backend.Repositories.Interfaces;

namespace backend.Services.Implementations;

public class StreamViewerCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StreamViewerCleanupService> _logger;

    public StreamViewerCleanupService(IServiceProvider serviceProvider, ILogger<StreamViewerCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var viewerRepository = scope.ServiceProvider.GetRequiredService<IStreamViewerRepository>();
                await viewerRepository.CleanupStaleViewersAsync(TimeSpan.FromSeconds(90));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up stale stream viewers");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
