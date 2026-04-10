namespace backend.Services.Interfaces;

public interface IAudioFingerprintService
{
    /// <summary>
    /// Generate a SHA-256 hash of the audio file content for copyright comparison.
    /// </summary>
    Task<string> GenerateHashAsync(Stream audioStream);

    /// <summary>
    /// Check if a song with the same audio hash already exists.
    /// Returns the existing song ID if found, null otherwise.
    /// </summary>
    Task<Guid?> FindDuplicateAsync(string audioHash);
}
