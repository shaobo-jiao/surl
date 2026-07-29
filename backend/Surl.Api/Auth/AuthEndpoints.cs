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
            .WithValidationFilter<RegisterRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/login", LoginAsync)
            .WithValidationFilter<LoginRequest>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshAsync)
            .WithValidationFilter<RefreshRequest>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/change-password", ChangePasswordAsync)
            .WithValidationFilter<ChangePasswordRequest>()
            .WithDescription("Change password for authenticated user")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    public static async Task<Results<Created<UserResponse>, ProblemHttpResult>> RegisterAsync(
        IAuthService authService, [FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // return Created as of now. may add a /me endpoint to get current user info in the future
        var result = await authService.RegisterAsync(request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Created(string.Empty, result.Value!.ToResponse())
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }

    public static async Task<Results<Ok<LoginResponse>, ProblemHttpResult>> LoginAsync(
        IAuthService authService, LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Ok(result.Value!)
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }

    public static async Task<Results<Ok<RefreshResponse>, ProblemHttpResult>> RefreshAsync(
        IAuthService authService, RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Ok(result.Value!)
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }

    public static async Task<Results<Ok<string>, ProblemHttpResult>> ChangePasswordAsync(
        IAuthService authService, ClaimsPrincipal user, ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var result = await authService.ChangePasswordAsync(userId, request, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Ok("Password changed successfully")
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }
}
