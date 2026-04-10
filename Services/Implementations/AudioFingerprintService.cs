using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class AudioFingerprintService : IAudioFingerprintService
{
    private readonly ApplicationDbContext _context;

    public AudioFingerprintService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateHashAsync(Stream audioStream)
    {
        audioStream.Position = 0;
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(audioStream);
        audioStream.Position = 0; // Reset for subsequent use
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<Guid?> FindDuplicateAsync(string audioHash)
    {
        var existingSong = await _context.Songs
            .Where(s => s.AudioHash == audioHash && s.IsActive)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync();

        return existingSong;
    }
}
