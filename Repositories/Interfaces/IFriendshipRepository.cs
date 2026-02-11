using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IFriendshipRepository : IRepository<Friendship>
{
    Task<IEnumerable<User>> GetFriendsAsync(Guid userId);
    Task<IEnumerable<User>> GetLiveFriendsAsync(Guid userId);
    Task<bool> AreFriendsAsync(Guid userId1, Guid userId2);
    Task<Friendship?> GetFriendshipAsync(Guid userId1, Guid userId2);
}
