using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class FriendRequestRepository : Repository<FriendRequest>, IFriendRequestRepository
{
    public FriendRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FriendRequest>> GetPendingRequestsForUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(fr => fr.Sender)
            .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FriendRequest>> GetSentRequestsByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(fr => fr.Receiver)
            .Where(fr => fr.SenderId == userId)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync();
    }

    public async Task<FriendRequest?> GetRequestBetweenUsersAsync(Guid senderId, Guid receiverId)
    {
        return await _dbSet.FirstOrDefaultAsync(fr => 
            fr.SenderId == senderId && fr.ReceiverId == receiverId);
    }

    public async Task DeleteRequestsBetweenUsersAsync(Guid userId1, Guid userId2)
    {
        var requests = await _dbSet
            .Where(fr => 
                (fr.SenderId == userId1 && fr.ReceiverId == userId2) ||
                (fr.SenderId == userId2 && fr.ReceiverId == userId1))
            .ToListAsync();
        
        _dbSet.RemoveRange(requests);
        await _context.SaveChangesAsync();
    }
}

