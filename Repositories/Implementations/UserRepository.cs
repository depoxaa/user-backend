using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetByUsernameWithDetailsAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Playlists)
                .ThenInclude(p => p.PlaylistSongs)
            .Include(u => u.FriendshipsInitiated)
            .Include(u => u.FriendshipsReceived)
            .Include(u => u.SubscribedArtists)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetWithPlaylistsAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.Playlists)
                .ThenInclude(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
            .Include(u => u.FriendshipsInitiated)
            .Include(u => u.FriendshipsReceived)
            .Include(u => u.SubscribedArtists)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetWithFriendsAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.FriendshipsInitiated)
                .ThenInclude(f => f.Friend)
            .Include(u => u.FriendshipsReceived)
                .ThenInclude(f => f.User)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string query, int take = 20)
    {
        var lowerQuery = query.ToLower();
        return await _dbSet
            .Where(u => u.Username.ToLower().Contains(lowerQuery) || u.Email.ToLower().Contains(lowerQuery))
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetOnlineUsersAsync()
    {
        return await _dbSet.Where(u => u.IsOnline).ToListAsync();
    }
}
