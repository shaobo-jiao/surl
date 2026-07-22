using Surl.Api.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Surl.Api.Data;

namespace Surl.Api.Users;

public class UserService(AppDbContext dbContext, IPasswordHasher<User> passwordHasher, TimeProvider timeProvider) : IUserService
{
    private AppDbContext _dbContext = dbContext;
    private IPasswordHasher<User> _passwordHasher = passwordHasher;
    private TimeProvider _timeProvider = timeProvider;

    public async Task<User> CreateUserAsync(CreateUserRequest createUserRequest)
    {
        // basic validation done at endpoint filter; check if username already exists here
        if (await UserExistsAsync(createUserRequest.Username))
            throw new DuplicateUsernameException();

        var user = new User(createUserRequest.Username)
        {
            CreatedAt = _timeProvider.GetUtcNow(),
            Email = createUserRequest.Email,
            // registered users always have role "User", while "Admins" can manage users.  
            Role = createUserRequest.Role,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, createUserRequest.Password);

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task DeleteUserAsync(int id)
    {
        User user = await GetUserByIdAsync(id) ?? throw new UserNotFoundException();
        // hard-delete users. later may change to soft delete by introducing an IsDeleted flag in the user entity.
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Common.PagedResult<User>> GetUsersAsync(QueryUserParams queryParams, CancellationToken cancellationToken)
    {
        var userQuery = _dbContext.Users.AsQueryable();

        // filter username & email
        if (!string.IsNullOrEmpty(queryParams.Username))
            userQuery = userQuery.Where(u => u.Username.Contains(queryParams.Username));

        if (!string.IsNullOrEmpty(queryParams.Email))
            userQuery = userQuery.Where(u => u.Email != null && u.Email.Contains(queryParams.Email));

        // order 
        userQuery = userQuery.OrderBy($"{queryParams.OrderBy ?? nameof(User.Id)} {((queryParams.IsDescending ?? false) ? "DESC" : "ASC")}");

        // pagination
        return await userQuery.ToPagedResultAsync(queryParams.Page, queryParams.PageSize, cancellationToken);
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> UpdateUserAsync(int id, UpdateUserRequest updateRequest)
    {
        User user = await GetUserByIdAsync(id) ?? throw new UserNotFoundException();
        // check if username is being updated to an existing one
        if (user.Username != updateRequest.Username && await UserExistsAsync(updateRequest.Username))
            throw new DuplicateUsernameException();

        user.Username = updateRequest.Username;
        user.PasswordHash = _passwordHasher.HashPassword(user, updateRequest.Password);
        user.Email = updateRequest.Email;

        await _dbContext.SaveChangesAsync();
        return user;
    }

    public Task<bool> UserExistsAsync(string username)
    {
        return _dbContext.Users.AnyAsync(u => u.Username == username);
    }
}
