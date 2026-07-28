using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.Common;
using Surl.Api.Users;

namespace Surl.Api.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", RegisterAsync)
            .WithValidationFilter<RegisterRequest>();

        group.MapPost("/login", LoginAsync)
            .WithValidationFilter<LoginRequest>();

        group.MapPost("/refresh", RefreshAsync)
            .WithValidationFilter<RefreshRequest>();

        group.MapPost("/change-password", ChangePasswordAsync)
            .WithValidationFilter<ChangePasswordRequest>()
            .WithDescription("Change password for authenticated user")
            .RequireAuthorization();
    }

    public static async Task<Results<Created<UserResponse>, BadRequest<string>>> RegisterAsync(
        IAuthService authService, [FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // return Created as of now. may add a /me endpoint to get current user info in the future
        var result = await authService.RegisterAsync(request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Created(string.Empty, result.Value!.ToResponse())
            : TypedResults.BadRequest("Username already exists"); // the only return error type
    }

    public static async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> LoginAsync(
        IAuthService authService, LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Ok(result.Value!)
            : TypedResults.Unauthorized();
    }

    public static async Task<Results<Ok<RefreshResponse>, UnauthorizedHttpResult>> RefreshAsync(
        IAuthService authService, RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Ok(result.Value!)
            : TypedResults.Unauthorized();
    }

    public static async Task<Results<Ok<string>, BadRequest<string>, UnauthorizedHttpResult>> ChangePasswordAsync(
        IAuthService authService, ClaimsPrincipal user, ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var result = await authService.ChangePasswordAsync(userId, request, cancellationToken);
        if (result.IsSuccessful)
            return TypedResults.Ok("Password changed successfully");

        return result.Error switch
        {
            AuthError.UserNotFound => TypedResults.BadRequest("User not found"),
            AuthError.InvalidCredentials => TypedResults.BadRequest("Wrong old password"),
            _ => TypedResults.BadRequest("Unable to change password"),
        };
    }
}
