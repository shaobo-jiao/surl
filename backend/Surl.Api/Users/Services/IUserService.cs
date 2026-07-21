using Surl.Api.Common;

namespace Surl.Api.Users;

public interface IUserService
{
    /// <summary>
    /// Get the list of users as a paged result. 
    /// </summary>
    /// <param name="queryParams">parameters controlling the query, such as filters and pagination parameters</param>
    /// <returns>The list of user wrapped in a paged result</returns>
    Task<PagedResult<User>> GetUsersAsync(QueryUserParams queryParams);

    /// <summary>
    /// Get certain user by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<User?> GetUserByIdAsync(int id);

    /// <summary>
    /// Gets certain user by username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    Task<User?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Creates a new user. 
    /// </summary>
    /// <param name="createRequest"></param>
    /// <returns></returns>
    Task<User> CreateUserAsync(CreateUserRequest createRequest);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateRequest"></param>
    /// <returns></returns>
    Task<User> UpdateUserAsync(int id, UpdateUserRequest updateRequest);

    /// <summary>
    /// Deletes a user based on id. 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteUserAsync(int id);

    /// <summary>
    /// Checks whether there's a user with given username.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    Task<bool> UserExistsAsync(string username);
}
