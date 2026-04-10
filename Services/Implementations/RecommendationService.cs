using AutoMapper;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs.Song;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly ISongLikeRepository _songLikeRepository;
    private readonly ISongPurchaseRepository _songPurchaseRepository;
    private readonly IMapper _mapper;

    public RecommendationService(
        ApplicationDbContext context,
        ISongLikeRepository songLikeRepository,
        ISongPurchaseRepository songPurchaseRepository,
        IMapper mapper)
    {
        _context = context;
        _songLikeRepository = songLikeRepository;
        _songPurchaseRepository = songPurchaseRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SongDto>> GetRecommendationsAsync(Guid userId, int limit = 20, int offset = 0)
    {
        // Compute recommendations entirely on the database side using a combined
        // recency × frequency score. The score is:
        //   playCount * (1.0 / (1.0 + daysSinceLastPlay))
        // This favours tracks that are both frequently played AND recently played.
        var now = DateTime.UtcNow;

        // Fetch grouped play data, then compute score in memory.
        // PostgreSQL doesn't support DateDiffDay, so we pull aggregates
        // and compute the recency × frequency score client-side.
        var playData = await _context.SongPlays
            .Where(sp => sp.UserId == userId && sp.Song.IsActive)
            .GroupBy(sp => sp.SongId)
            .Select(g => new
            {
                SongId = g.Key,
                PlayCount = g.Count(),
                LastPlayedAt = g.Max(sp => sp.PlayedAt)
            })
            .ToListAsync();

        var songs = playData
            .Select(g => new
            {
                g.SongId,
                g.PlayCount,
                g.LastPlayedAt,
                Score = g.PlayCount * (1.0 / (1.0 + (now - g.LastPlayedAt).TotalDays))
            })
            .OrderByDescending(x => x.Score)
            .Skip(offset)
            .Take(limit)
            .ToList();

        var songIds = songs.Select(s => s.SongId).ToList();

        var songEntities = await _context.Songs
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.Genre)
            .Where(s => songIds.Contains(s.Id) && s.IsActive)
            .ToListAsync();

        // Maintain the score order
        var orderedEntities = songIds
            .Select(id => songEntities.FirstOrDefault(s => s.Id == id))
            .Where(s => s != null)
            .ToList();

        var dtos = _mapper.Map<List<SongDto>>(orderedEntities);
        var purchasedIds = (await _songPurchaseRepository.GetPurchasedSongIdsByUserAsync(userId)).ToHashSet();

        foreach (var dto in dtos)
        {
            dto.IsLiked = await _songLikeRepository.HasUserLikedSongAsync(userId, dto.Id);
            dto.IsPurchased = dto.IsFree || purchasedIds.Contains(dto.Id);
        }

        return dtos;
    }

    public async Task<int> GetPlayEventCountAsync(Guid userId)
    {
        return await _context.SongPlays.CountAsync(sp => sp.UserId == userId);
    }
}
