using AutoMapper;
using backend.DTOs.Friend;
using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class FriendService : IFriendService
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ISseConnectionManager _sseManager;

    public FriendService(
        IFriendshipRepository friendshipRepository,
        IFriendRequestRepository friendRequestRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ISseConnectionManager sseManager)
    {
        _friendshipRepository = friendshipRepository;
        _friendRequestRepository = friendRequestRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _sseManager = sseManager;
    }

    public async Task<IEnumerable<FriendDto>> GetFriendsAsync(Guid userId)
    {
        var friends = await _friendshipRepository.GetFriendsAsync(userId);
        return _mapper.Map<IEnumerable<FriendDto>>(friends);
    }

    public async Task<IEnumerable<FriendDto>> GetLiveFriendsAsync(Guid userId)
    {
        var liveFriends = await _friendshipRepository.GetLiveFriendsAsync(userId);
        return _mapper.Map<IEnumerable<FriendDto>>(liveFriends);
    }

    public async Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(Guid userId)
    {
        var requests = await _friendRequestRepository.GetPendingRequestsForUserAsync(userId);
        return _mapper.Map<IEnumerable<FriendRequestDto>>(requests);
    }

    public async Task<IEnumerable<FriendRequestDto>> GetSentRequestsAsync(Guid userId)
    {
        var requests = await _friendRequestRepository.GetSentRequestsByUserAsync(userId);
        return _mapper.Map<IEnumerable<FriendRequestDto>>(requests);
    }

    public async Task SendRequestAsync(Guid senderId, Guid receiverId)
    {
        if (senderId == receiverId)
        {
            throw new InvalidOperationException("You cannot send a friend request to yourself");
        }

        var receiver = await _userRepository.GetByIdAsync(receiverId);
        if (receiver == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if already friends
        if (await _friendshipRepository.AreFriendsAsync(senderId, receiverId))
        {
            throw new InvalidOperationException("You are already friends");
        }

        // Check if request already exists
        var existingRequest = await _friendRequestRepository.GetRequestBetweenUsersAsync(senderId, receiverId);
        if (existingRequest != null)
        {
            throw new InvalidOperationException("Friend request already sent");
        }

        // Check if there's a pending request from the other user
        var reverseRequest = await _friendRequestRepository.GetRequestBetweenUsersAsync(receiverId, senderId);
        if (reverseRequest != null && reverseRequest.Status == FriendRequestStatus.Pending)
        {
            // Auto-accept
            await AcceptRequestInternal(reverseRequest);
            return;
        }

        var request = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending
        };

        await _friendRequestRepository.AddAsync(request);

        // Notify receiver about new friend request via SSE
        await _sseManager.SendEventAsync(receiverId, "friendRequest", new { action = "received" });
    }

    public async Task AcceptRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _friendRequestRepository.GetByIdAsync(requestId);
        if (request == null)
        {
            throw new InvalidOperationException("Friend request not found");
        }

        if (request.ReceiverId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to accept this request");
        }

        if (request.Status != FriendRequestStatus.Pending)
        {
            throw new InvalidOperationException("This request has already been processed");
        }

        await AcceptRequestInternal(request);
    }

    public async Task RejectRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _friendRequestRepository.GetByIdAsync(requestId);
        if (request == null)
        {
            throw new InvalidOperationException("Friend request not found");
        }

        if (request.ReceiverId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to reject this request");
        }

        if (request.Status != FriendRequestStatus.Pending)
        {
            throw new InvalidOperationException("This request has already been processed");
        }

        request.Status = FriendRequestStatus.Rejected;
        await _friendRequestRepository.UpdateAsync(request);

        // Notify sender that request was rejected via SSE
        await _sseManager.SendEventAsync(request.SenderId, "friendRequest", new { action = "rejected" });
    }

    public async Task RemoveFriendAsync(Guid userId, Guid friendId)
    {
        var friendship = await _friendshipRepository.GetFriendshipAsync(userId, friendId);
        if (friendship == null)
        {
            throw new InvalidOperationException("Friendship not found");
        }

        await _friendshipRepository.DeleteAsync(friendship);
        
        // Also delete any friend requests between these users so they can re-add each other
        await _friendRequestRepository.DeleteRequestsBetweenUsersAsync(userId, friendId);

        // Notify both users about friend removal via SSE
        await _sseManager.SendEventToUsersAsync(new[] { userId, friendId }, "friends", new { action = "removed" });
    }

    public async Task<bool> AreFriendsAsync(Guid userId1, Guid userId2)
    {
        return await _friendshipRepository.AreFriendsAsync(userId1, userId2);
    }

    private async Task AcceptRequestInternal(FriendRequest request)
    {
        request.Status = FriendRequestStatus.Accepted;
        await _friendRequestRepository.UpdateAsync(request);

        // Create friendship
        var friendship = new Friendship
        {
            UserId = request.SenderId,
            FriendId = request.ReceiverId
        };

        await _friendshipRepository.AddAsync(friendship);

        // Notify both users about new friendship via SSE
        await _sseManager.SendEventToUsersAsync(
            new[] { request.SenderId, request.ReceiverId }, 
            "friends", 
            new { action = "added" });
    }
}
