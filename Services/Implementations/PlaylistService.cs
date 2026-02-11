using AutoMapper;
using backend.DTOs.Playlist;
using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class PlaylistService : IPlaylistService
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IPlaylistSongRepository _playlistSongRepository;
    private readonly ISongLikeRepository _songLikeRepository;
    private readonly IRepository<PlaylistView> _playlistViewRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IMapper _mapper;

    public PlaylistService(
        IPlaylistRepository playlistRepository,
        IPlaylistSongRepository playlistSongRepository,
        ISongLikeRepository songLikeRepository,
        IRepository<PlaylistView> playlistViewRepository,
        IFriendshipRepository friendshipRepository,
        IMapper mapper)
    {
        _playlistRepository = playlistRepository;
        _playlistSongRepository = playlistSongRepository;
        _songLikeRepository = songLikeRepository;
        _playlistViewRepository = playlistViewRepository;
        _friendshipRepository = friendshipRepository;
        _mapper = mapper;
    }

    public async Task<PlaylistDto?> GetByIdAsync(Guid id)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        return playlist == null ? null : _mapper.Map<PlaylistDto>(playlist);
    }

    public async Task<PlaylistDetailDto?> GetDetailAsync(Guid id, Guid? viewerId = null)
    {
        var playlist = await _playlistRepository.GetWithSongsAsync(id);
        if (playlist == null) return null;

        // Access control: check if viewer can access this playlist
        if (viewerId != playlist.UserId)
        {
            // Private playlists: only owner can access
            if (playlist.Status == PlaylistStatus.Private)
            {
                return null;
            }
            
            // Public playlists: only friends can access
            if (playlist.Status == PlaylistStatus.Public && viewerId.HasValue)
            {
                var areFriends = await _friendshipRepository.AreFriendsAsync(viewerId.Value, playlist.UserId);
                if (!areFriends)
                {
                    return null;
                }
            }
            else if (playlist.Status == PlaylistStatus.Public && !viewerId.HasValue)
            {
                // Non-authenticated users cannot view public playlists
                return null;
            }
        }

        var dto = _mapper.Map<PlaylistDetailDto>(playlist);
        
        // Map songs with like status
        var songs = new List<PlaylistSongDto>();
        foreach (var ps in playlist.PlaylistSongs.OrderBy(ps => ps.Order))
        {
            var songDto = new PlaylistSongDto
            {
                Order = ps.Order,
                SongId = ps.Song.Id,
                Title = ps.Song.Title,
                Artist = ps.Song.Artist.Name,
                Duration = FormatDuration(ps.Song.Duration),
                CoverArt = ps.Song.CoverArt,
                IsLiked = viewerId.HasValue && await _songLikeRepository.HasUserLikedSongAsync(viewerId.Value, ps.Song.Id)
            };
            songs.Add(songDto);
        }
        dto.Songs = songs;

        // Get recent viewers
        var views = await _playlistViewRepository.FindAsync(pv => pv.PlaylistId == id);
        dto.RecentViewers = views
            .OrderByDescending(v => v.ViewedAt)
            .Take(10)
            .Select(v => _mapper.Map<PlaylistViewerDto>(v))
            .ToList();

        // Record view if viewer is specified and not the owner
        if (viewerId.HasValue && viewerId.Value != playlist.UserId)
        {
            var existingView = await _playlistViewRepository.FindSingleAsync(
                pv => pv.PlaylistId == id && pv.UserId == viewerId.Value);
            
            if (existingView != null)
            {
                existingView.ViewedAt = DateTime.UtcNow;
                await _playlistViewRepository.UpdateAsync(existingView);
            }
            else
            {
                var view = new PlaylistView
                {
                    PlaylistId = id,
                    UserId = viewerId.Value,
                    ViewedAt = DateTime.UtcNow
                };
                await _playlistViewRepository.AddAsync(view);
                playlist.ViewCount++;
                await _playlistRepository.UpdateAsync(playlist);
            }
        }

        return dto;
    }

    public async Task<IEnumerable<PlaylistDto>> GetByUserAsync(Guid userId, Guid? viewerId = null)
    {
        var playlists = await _playlistRepository.GetByUserAsync(userId);
        
        // If viewer is the owner, return all playlists
        if (viewerId == userId)
        {
            return _mapper.Map<IEnumerable<PlaylistDto>>(playlists);
        }
        
        // If non-authenticated or not owner, apply access control
        if (!viewerId.HasValue)
        {
            // Non-authenticated users cannot see any playlists
            return Enumerable.Empty<PlaylistDto>();
        }
        
        // Check friendship for public playlist access
        var areFriends = await _friendshipRepository.AreFriendsAsync(viewerId.Value, userId);
        if (!areFriends)
        {
            // Non-friends cannot see any playlists
            return Enumerable.Empty<PlaylistDto>();
        }
        
        // Friends can only see public playlists (not private)
        var accessiblePlaylists = playlists.Where(p => p.Status == PlaylistStatus.Public);
        return _mapper.Map<IEnumerable<PlaylistDto>>(accessiblePlaylists);
    }

    public async Task<IEnumerable<PlaylistDto>> GetPublicPlaylistsAsync(int take = 20)
    {
        var playlists = await _playlistRepository.GetPublicPlaylistsAsync(take);
        return _mapper.Map<IEnumerable<PlaylistDto>>(playlists);
    }

    public async Task<IEnumerable<PlaylistDto>> SearchAsync(string query)
    {
        var playlists = await _playlistRepository.SearchPlaylistsAsync(query);
        return _mapper.Map<IEnumerable<PlaylistDto>>(playlists);
    }

    public async Task<PlaylistDto> CreateAsync(Guid userId, CreatePlaylistDto dto)
    {
        var playlist = new Playlist
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            Color = dto.Color,
            Status = Enum.Parse<PlaylistStatus>(dto.Status, true),
            UserId = userId
        };

        await _playlistRepository.AddAsync(playlist);
        return _mapper.Map<PlaylistDto>(playlist);
    }

    public async Task<PlaylistDto> UpdateAsync(Guid id, Guid userId, UpdatePlaylistDto dto)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
        {
            throw new InvalidOperationException("Playlist not found");
        }

        if (playlist.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this playlist");
        }

        if (!string.IsNullOrEmpty(dto.Name))
            playlist.Name = dto.Name;
        if (dto.Description != null)
            playlist.Description = dto.Description;
        if (!string.IsNullOrEmpty(dto.Icon))
            playlist.Icon = dto.Icon;
        if (!string.IsNullOrEmpty(dto.Color))
            playlist.Color = dto.Color;
        if (!string.IsNullOrEmpty(dto.Status))
            playlist.Status = Enum.Parse<PlaylistStatus>(dto.Status, true);

        await _playlistRepository.UpdateAsync(playlist);
        return _mapper.Map<PlaylistDto>(playlist);
    }

    public async Task<PlaylistDto> UpdateCoverAsync(Guid id, Guid userId, string coverImage)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
        {
            throw new InvalidOperationException("Playlist not found");
        }

        if (playlist.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this playlist");
        }

        playlist.CoverImage = coverImage;
        await _playlistRepository.UpdateAsync(playlist);
        return _mapper.Map<PlaylistDto>(playlist);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
        {
            throw new InvalidOperationException("Playlist not found");
        }

        if (playlist.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this playlist");
        }

        await _playlistRepository.DeleteAsync(playlist);
    }

    public async Task AddSongAsync(Guid playlistId, Guid userId, Guid songId)
    {
        var playlist = await _playlistRepository.GetByIdAsync(playlistId);
        if (playlist == null)
        {
            throw new InvalidOperationException("Playlist not found");
        }

        if (playlist.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this playlist");
        }

        if (await _playlistSongRepository.IsSongInPlaylistAsync(playlistId, songId))
        {
            throw new InvalidOperationException("Song is already in the playlist");
        }

        var maxOrder = await _playlistSongRepository.GetMaxOrderAsync(playlistId);

        var playlistSong = new PlaylistSong
        {
            PlaylistId = playlistId,
            SongId = songId,
            Order = maxOrder + 1
        };

        await _playlistSongRepository.AddAsync(playlistSong);
    }

    public async Task RemoveSongAsync(Guid playlistId, Guid userId, Guid songId)
    {
        var playlist = await _playlistRepository.GetByIdAsync(playlistId);
        if (playlist == null)
        {
            throw new InvalidOperationException("Playlist not found");
        }

        if (playlist.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this playlist");
        }

        var playlistSong = await _playlistSongRepository.FindSingleAsync(
            ps => ps.PlaylistId == playlistId && ps.SongId == songId);
        
        if (playlistSong == null)
        {
            throw new InvalidOperationException("Song not found in playlist");
        }

        await _playlistSongRepository.DeleteAsync(playlistSong);
    }

    public async Task ReorderSongsAsync(Guid playlistId, Guid userId, List<Guid> songIds)
    {
        var playlist = await _playlistRepository.GetByIdAsync(playlistId);
        if (playlist == null)
        {
            throw new InvalidOperationException("Playlist not found");
        }

        if (playlist.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this playlist");
        }

        var playlistSongs = await _playlistSongRepository.GetByPlaylistAsync(playlistId);
        var order = 1;

        foreach (var songId in songIds)
        {
            var ps = playlistSongs.FirstOrDefault(p => p.SongId == songId);
            if (ps != null)
            {
                ps.Order = order++;
                await _playlistSongRepository.UpdateAsync(ps);
            }
        }
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.Hours > 0 
            ? $"{duration.Hours}:{duration.Minutes:D2}:{duration.Seconds:D2}"
            : $"{duration.Minutes}:{duration.Seconds:D2}";
    }
}
