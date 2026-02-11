using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class FriendshipRepository : Repository<Friendship>, IFriendshipRepository
{
    public FriendshipRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<User>> GetFriendsAsync(Guid userId)
    {
        var friendships = await _dbSet
            .Include(f => f.User)
            .Include(f => f.Friend)
            .Where(f => f.UserId == userId || f.FriendId == userId)
            .ToListAsync();

        return friendships.Select(f => f.UserId == userId ? f.Friend : f.User);
    }

    public async Task<bool> AreFriendsAsync(Guid userId1, Guid userId2)
    {
        return await _dbSet.AnyAsync(f =>
            (f.UserId == userId1 && f.FriendId == userId2) ||
            (f.UserId == userId2 && f.FriendId == userId1));
    }

    public async Task<Friendship?> GetFriendshipAsync(Guid userId1, Guid userId2)
    {
        return await _dbSet.FirstOrDefaultAsync(f =>
            (f.UserId == userId1 && f.FriendId == userId2) ||
            (f.UserId == userId2 && f.FriendId == userId1));
    }

    public async Task<IEnumerable<User>> GetLiveFriendsAsync(Guid userId)
    {
        var friendships = await _dbSet
            .Include(f => f.User)
            .Include(f => f.Friend)
            .Where(f => f.UserId == userId || f.FriendId == userId)
            .ToListAsync();

        // Return friends who have a live status (starting with LIVE:)
        return friendships
            .Select(f => f.UserId == userId ? f.Friend : f.User)
            .Where(u => !string.IsNullOrEmpty(u.CurrentlyListeningStatus) && 
                        u.CurrentlyListeningStatus.Contains("LIVE"));
    }
}
