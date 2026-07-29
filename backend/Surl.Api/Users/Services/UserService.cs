using Surl.Api.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Surl.Api.Data;

namespace Surl.Api.Users;

public class UserService(AppDbContext dbContext, IPasswordHasher<User> passwordHasher,
    TimeProvider timeProvider, ILogger<UserService> logger) : IUserService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<UserService> _logger = logger;

    public async Task<Common.PagedResult<User>> GetUsersAsync(QueryUserParams queryParams, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting users page {Page} with page size {PageSize}", 
            queryParams.Page, queryParams.PageSize);

        var userQuery = _dbContext.Users.AsQueryable();

        // filter username & email
        if (!string.IsNullOrEmpty(queryParams.Username))
            userQuery = userQuery.Where(u => u.Username.Contains(queryParams.Username));

        if (!string.IsNullOrEmpty(queryParams.Email))
            userQuery = userQuery.Where(u => u.Email != null && u.Email.Contains(queryParams.Email));

        // order
        bool IsDescending = queryParams.IsDescending ?? false;
        userQuery = queryParams.OrderBy switch
        {
            QueryUserParamsAllowedOrderByProperties.Username => userQuery.OrderByDirection(x => x.Username, IsDescending),
            QueryUserParamsAllowedOrderByProperties.Email => userQuery.OrderByDirection(x => x.Email, IsDescending),
            QueryUserParamsAllowedOrderByProperties.CreatedAt => userQuery.OrderByDirection(x => x.CreatedAt, IsDescending),
            _ => userQuery.OrderByDirection(x => x.Id, IsDescending),
        };

        // pagination
        return await userQuery.ToPagedResultAsync(queryParams.Page, queryParams.PageSize, cancellationToken);
    }

    public Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting user {UserId}", id);
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting user by username");
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken)
    {
        return await GetUserByUsernameAsync(username, cancellationToken) != null;
    }

    public async Task<Result<User, UserError>> CreateUserAsync(
        CreateUserRequest createUserRequest, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating user");

        // check username duplication
        if (await UserExistsAsync(createUserRequest.Username, cancellationToken))
        {
            _logger.LogDebug("User creation rejected because the username already exists");
            return Result<User, UserError>.Failure(UserError.DuplicateUsername);
        }

        var user = new User(createUserRequest.Username)
        {
            CreatedAt = _timeProvider.GetUtcNow(),
            Email = createUserRequest.Email,
            // registered users always have role "User", while "Admins" can manage users.  
            Role = createUserRequest.Role,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, createUserRequest.Password);

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} created", user.Id);
        return Result<User, UserError>.Success(user);
    }

    public async Task<Result<User, UserError>> UpdateUserAsync(
        int id, UpdateUserRequest updateRequest, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating user {UserId}", id);

        var user = await GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            _logger.LogDebug("User update rejected because user {UserId} was not found", id);
            return Result<User, UserError>.Failure(UserError.UserNotFound);
        }

        // check if username is being updated to an existing one
        if (user.Username != updateRequest.Username &&
            await UserExistsAsync(updateRequest.Username, cancellationToken))
        {
            _logger.LogDebug("User update rejected because the new username already exists for user {UserId}", id);
            return Result<User, UserError>.Failure(UserError.DuplicateUsername);
        }

        user.Username = updateRequest.Username;
        user.PasswordHash = _passwordHasher.HashPassword(user, updateRequest.Password);
        user.Email = updateRequest.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} updated", id);
        return Result<User, UserError>.Success(user);
    }

    public async Task<Result<User, UserError>> DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting user {UserId}", id);

        var user = await GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            _logger.LogDebug("User deletion rejected because user {UserId} was not found", id);
            return Result<User, UserError>.Failure(UserError.UserNotFound);
        }

        // hard-delete users. later may change to soft delete by introducing an IsDeleted flag in the user entity.
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} deleted", id);
        return Result<User, UserError>.Success(user);
    }
}
