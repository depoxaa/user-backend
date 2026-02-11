using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IFriendRequestRepository : IRepository<FriendRequest>
{
    Task<IEnumerable<FriendRequest>> GetPendingRequestsForUserAsync(Guid userId);
    Task<IEnumerable<FriendRequest>> GetSentRequestsByUserAsync(Guid userId);
    Task<FriendRequest?> GetRequestBetweenUsersAsync(Guid senderId, Guid receiverId);
    Task DeleteRequestsBetweenUsersAsync(Guid userId1, Guid userId2);
}

