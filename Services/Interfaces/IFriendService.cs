using backend.DTOs.Friend;

namespace backend.Services.Interfaces;

public interface IFriendService
{
    Task<IEnumerable<FriendDto>> GetFriendsAsync(Guid userId);
    Task<IEnumerable<FriendDto>> GetLiveFriendsAsync(Guid userId);
    Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(Guid userId);
    Task<IEnumerable<FriendRequestDto>> GetSentRequestsAsync(Guid userId);
    Task SendRequestAsync(Guid senderId, Guid receiverId);
    Task AcceptRequestAsync(Guid requestId, Guid userId);
    Task RejectRequestAsync(Guid requestId, Guid userId);
    Task RemoveFriendAsync(Guid userId, Guid friendId);
    Task<bool> AreFriendsAsync(Guid userId1, Guid userId2);
}
