using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameWithDetailsAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetWithPlaylistsAsync(Guid id);
    Task<User?> GetWithFriendsAsync(Guid id);
    Task<IEnumerable<User>> SearchUsersAsync(string query, int take = 20);
    Task<IEnumerable<User>> GetOnlineUsersAsync();
}
