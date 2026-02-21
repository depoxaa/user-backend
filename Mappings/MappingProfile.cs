using AutoMapper;
using backend.DTOs.Auth;
using backend.DTOs.Song;
using backend.DTOs.Playlist;
using backend.DTOs.Friend;
using backend.DTOs.Album;
using backend.Entities;

namespace backend.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FriendsCount, opt => opt.MapFrom(src => 
                src.FriendshipsInitiated.Count + src.FriendshipsReceived.Count))
            .ForMember(dest => dest.SubscribedArtistsCount, opt => opt.MapFrom(src => 
                src.SubscribedArtists.Count))
            .ForMember(dest => dest.TotalSongsInPlaylists, opt => opt.MapFrom(src => 
                src.Playlists.Sum(p => p.PlaylistSongs.Count)));

        CreateMap<User, UserInfoDto>();
        CreateMap<User, UserInfoForFriendDto>();

        // Artist mappings
        CreateMap<Artist, ArtistDto>()
            .ForMember(dest => dest.TotalSongs, opt => opt.MapFrom(src => src.Songs.Count))
            .ForMember(dest => dest.TotalPlays, opt => opt.MapFrom(src => src.Songs.Sum(s => s.TotalPlays)))
            .ForMember(dest => dest.TotalLikes, opt => opt.MapFrom(src => src.Songs.Sum(s => s.TotalLikes)))
            .ForMember(dest => dest.SubscribersCount, opt => opt.MapFrom(src => src.Subscribers.Count));

        CreateMap<Artist, ArtistInfoDto>();

        // Song mappings
        CreateMap<Song, SongDto>()
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => FormatDuration(src.Duration)))
            .ForMember(dest => dest.TotalListeningTime, opt => opt.MapFrom(src => FormatListeningTime(src.TotalListeningSeconds)))
            .ForMember(dest => dest.IsPurchased, opt => opt.Ignore());

        CreateMap<Song, PlaylistSongDto>()
            .ForMember(dest => dest.SongId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Artist.Name))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => FormatDuration(src.Duration)));

        // Genre mappings
        CreateMap<Genre, GenreInfoDto>();

        // Album mappings
        CreateMap<Album, AlbumInfoDto>();
        CreateMap<Album, AlbumDto>()
            .ForMember(dest => dest.SongsCount, opt => opt.MapFrom(src => src.Songs.Count));
        CreateMap<Album, AlbumDetailDto>()
            .ForMember(dest => dest.SongsCount, opt => opt.MapFrom(src => src.Songs.Count));

        // Playlist mappings
        CreateMap<Playlist, PlaylistDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TracksCount, opt => opt.MapFrom(src => src.PlaylistSongs.Count))
            .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.User));

        CreateMap<Playlist, PlaylistDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TracksCount, opt => opt.MapFrom(src => src.PlaylistSongs.Count))
            .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Songs, opt => opt.Ignore())
            .ForMember(dest => dest.RecentViewers, opt => opt.Ignore());

        // Friend mappings
        CreateMap<User, FriendDto>()
            .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => src.CurrentSongId.HasValue))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 
                src.CurrentSongId.HasValue 
                    ? "🎵 Listening now" 
                    : (src.LastSeen.HasValue ? $"Last seen {GetTimeAgo(src.LastSeen.Value)}" : "Offline")))
            .ForMember(dest => dest.PlaylistsCount, opt => opt.MapFrom(src => src.Playlists.Count));

        CreateMap<FriendRequest, FriendRequestDto>()
            .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.Sender))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => GetTimeAgo(src.CreatedAt)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // PlaylistView mappings
        CreateMap<PlaylistView, PlaylistViewerDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar))
            .ForMember(dest => dest.LastViewed, opt => opt.MapFrom(src => GetTimeAgo(src.ViewedAt)));
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.Hours > 0 
            ? $"{duration.Hours}:{duration.Minutes:D2}:{duration.Seconds:D2}"
            : $"{duration.Minutes}:{duration.Seconds:D2}";
    }

    private static string FormatListeningTime(long totalSeconds)
    {
        var hours = totalSeconds / 3600;
        if (hours >= 1000)
            return $"{hours:N0} hrs";
        return $"{hours} hrs";
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;
        
        if (span.TotalMinutes < 1)
            return "Just now";
        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes} minutes ago";
        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours} hours ago";
        if (span.TotalDays < 7)
            return $"{(int)span.TotalDays} days ago";
        
        return dateTime.ToString("MMM d, yyyy");
    }
}
