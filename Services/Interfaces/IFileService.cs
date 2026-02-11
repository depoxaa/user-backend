namespace backend.Services.Interfaces;

public interface IFileService
{
    Task<string> SaveAudioFileAsync(Stream fileStream, string fileName, Guid artistId);
    Task<string> SaveImageFileAsync(Stream fileStream, string fileName, string folder);
    Task DeleteFileAsync(string filePath);
    Stream? GetFileStream(string filePath);
    string GetContentType(string fileName);
    TimeSpan GetAudioDuration(string filePath);
}
