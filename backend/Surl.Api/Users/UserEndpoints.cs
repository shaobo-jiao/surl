using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.Common;

namespace Surl.Api.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // Admins can access user endpoints
        var group = app.MapGroup("/users")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
            
        group.MapGet("/", GetUsersAsync)
            .WithValidationFilter<QueryUserParams>();

        group.MapGet("/{id:int}", GetUserByIdAsync)
            .WithName("GetUserById");

        group.MapPost("/", CreateUserAsync)
            .WithValidationFilter<CreateUserRequest>();

        group.MapPut("/{id:int}", UpdateUserAsync)
            .WithValidationFilter<UpdateUserRequest>();

        group.MapDelete("/{id:int}", DeleteUserAsync);
    }

    public static async Task<Ok<PagedResult<UserResponse>>> GetUsersAsync(
        IUserService userService, [AsParameters] QueryUserParams queryUserParams, CancellationToken cancellationToken)
    {
        PagedResult<User> users = await userService.GetUsersAsync(queryUserParams, cancellationToken);
        return TypedResults.Ok(users.Map(u => u.ToResponse()));
    }

    public static async Task<Results<Ok<UserResponse>, NotFound>> GetUserByIdAsync(
        IUserService userService, [FromRoute] int id)
    {
        User? user = await userService.GetUserByIdAsync(id);
        if (user == null) return TypedResults.NotFound();
        return TypedResults.Ok(user.ToResponse());
    }

    public static async Task<Results<CreatedAtRoute<UserResponse>, BadRequest<string>>> CreateUserAsync(
        IUserService userService, [FromBody] CreateUserRequest request)
    {
        try
        {
            User user = await userService.CreateUserAsync(request);
            return TypedResults.CreatedAtRoute(user.ToResponse(), "GetUserById", new { id = user.Id });
        }
        catch (UserException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public static async Task<Results<Ok<UserResponse>, NotFound, BadRequest<string>>> UpdateUserAsync(
        IUserService userService, [FromRoute] int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            User user = await userService.UpdateUserAsync(id, request);
            return TypedResults.Ok(user.ToResponse());
        }
        catch (UserNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (UserException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public static async Task<Results<NoContent, NotFound>> DeleteUserAsync(
        IUserService userService, [FromRoute] int id)
    {
        try
        {
            await userService.DeleteUserAsync(id);
            return TypedResults.NoContent();
        }
        catch (UserNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
