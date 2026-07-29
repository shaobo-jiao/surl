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
            .WithValidationFilter<CreateUserRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", UpdateUserAsync)
            .WithValidationFilter<UpdateUserRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", DeleteUserAsync)
            .ProducesProblem(StatusCodes.Status404NotFound);
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
        return user == null
            ? TypedResults.NotFound()
            : TypedResults.Ok(user.ToResponse());
    }

    public static async Task<Results<CreatedAtRoute<UserResponse>, ProblemHttpResult>> CreateUserAsync(
        IUserService userService, [FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.CreateUserAsync(request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.CreatedAtRoute(
                result.Value!.ToResponse(), "GetUserById", new { id = result.Value!.Id })
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }

    public static async Task<Results<Ok<UserResponse>, ProblemHttpResult>> UpdateUserAsync(
        IUserService userService, [FromRoute] int id, [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userService.UpdateUserAsync(id, request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Ok(result.Value!.ToResponse())
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> DeleteUserAsync(
        IUserService userService, [FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await userService.DeleteUserAsync(id, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.NoContent()
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }
}
