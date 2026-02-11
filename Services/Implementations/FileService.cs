using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class FileService : IFileService
{
    private readonly string _basePath;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
    {
        _basePath = Path.Combine(env.ContentRootPath, "Storage");
        _logger = logger;
        
        // Ensure directories exist
        Directory.CreateDirectory(Path.Combine(_basePath, "audio"));
        Directory.CreateDirectory(Path.Combine(_basePath, "images", "covers"));
        Directory.CreateDirectory(Path.Combine(_basePath, "images", "avatars"));
        Directory.CreateDirectory(Path.Combine(_basePath, "images", "profiles"));
    }

    public async Task<string> SaveAudioFileAsync(Stream fileStream, string fileName, Guid artistId)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        var allowedExtensions = new[] { ".mp3", ".wav", ".flac", ".ogg", ".m4a" };
        
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Invalid audio file format");
        }

        var artistFolder = Path.Combine(_basePath, "audio", artistId.ToString());
        Directory.CreateDirectory(artistFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(artistFolder, uniqueFileName);

        using var file = File.Create(filePath);
        await fileStream.CopyToAsync(file);

        return $"audio/{artistId}/{uniqueFileName}";
    }

    public async Task<string> SaveImageFileAsync(Stream fileStream, string fileName, string folder)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Invalid image file format");
        }

        var targetFolder = Path.Combine(_basePath, "images", folder);
        Directory.CreateDirectory(targetFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(targetFolder, uniqueFileName);

        using var file = File.Create(filePath);
        await fileStream.CopyToAsync(file);

        return $"images/{folder}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public Stream? GetFileStream(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    public TimeSpan GetAudioDuration(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        
        // Simple estimation based on file size for mp3
        // In production, use NAudio or TagLib# for accurate duration
        try
        {
            var fileInfo = new FileInfo(fullPath);
            var fileSizeInKb = fileInfo.Length / 1024.0;
            // Average bitrate assumption: 192 kbps
            var durationInSeconds = (fileSizeInKb * 8) / 192;
            return TimeSpan.FromSeconds(durationInSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not determine audio duration for {FilePath}", filePath);
            return TimeSpan.FromMinutes(3); // Default fallback
        }
    }
}
