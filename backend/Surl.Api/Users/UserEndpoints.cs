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
        IUserService userService, [FromRoute] int id, CancellationToken cancellationToken)
    {
        User? user = await userService.GetUserByIdAsync(id, cancellationToken);
        if (user == null) return TypedResults.NotFound();
        return TypedResults.Ok(user.ToResponse());
    }

    public static async Task<Results<CreatedAtRoute<UserResponse>, BadRequest<string>>> CreateUserAsync(
        IUserService userService, [FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.CreateUserAsync(request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.CreatedAtRoute(
                result.Value!.ToResponse(), "GetUserById", new { id = result.Value!.Id })
            : TypedResults.BadRequest("Username already exists");
    }

    public static async Task<Results<Ok<UserResponse>, NotFound, BadRequest<string>>> UpdateUserAsync(
        IUserService userService, [FromRoute] int id, [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userService.UpdateUserAsync(id, request, cancellationToken);
        if (result.IsSuccessful)
            return TypedResults.Ok(result.Value!.ToResponse());

        return result.Error switch
        {
            UserError.UserNotFound => TypedResults.NotFound(),
            UserError.DuplicateUsername => TypedResults.BadRequest("Username already exists"),
            _ => TypedResults.BadRequest("Unable to update user"),
        };
    }

    public static async Task<Results<NoContent, NotFound>> DeleteUserAsync(
        IUserService userService, [FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await userService.DeleteUserAsync(id, cancellationToken);
        return result.IsSuccessful ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
