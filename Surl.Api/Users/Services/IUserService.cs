using Surl.Api.Common;

namespace Surl.Api.Users;

public interface IUserService
{
    Task<PagedResult<User>> GetUsersAsync(QueryUserParams queryParams);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User> CreateUserAsync(CreateUserRequest createRequest);
    Task<User> UpdateUserAsync(int id, UpdateUserRequest updateRequest);
    Task DeleteUserAsync(int id);
    Task<bool> UserExistsAsync(string username);
}
