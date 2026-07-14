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
        var group = app.MapGroup("/api/auth");

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

    public static async Task<Results<CreatedAtRoute<UserResponse>, BadRequest<string>>> RegisterAsync(
        IAuthService authService, [FromBody] RegisterRequest request)
    {
        try
        {
            User user = await authService.RegisterAsync(request);
            return TypedResults.CreatedAtRoute(user.ToResponse(), "GetUserById", new { id = user.Id });
        }
        catch (UserException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public static async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> LoginAsync(
        IAuthService authService, LoginRequest request)
    {
        try
        {
            LoginResponse response = await authService.LoginAsync(request);
            return TypedResults.Ok(response);
        }
        catch (Exception ex) when (ex is UserNotFoundException || ex is InvalidCredentialsException)
        {
            return TypedResults.Unauthorized();
        }
    }

    public static async Task<Results<Ok<RefreshResponse>, UnauthorizedHttpResult>> RefreshAsync(
        IAuthService authService, RefreshRequest request)
    {
        try
        {
            RefreshResponse response = await authService.RefreshAsync(request);
            return TypedResults.Ok(response);
        }
        catch (InvalidRefreshTokenException)
        {
            return TypedResults.Unauthorized();
        }
    }

    public static async Task<Results<Ok<string>, BadRequest<string>, UnauthorizedHttpResult>> ChangePasswordAsync(
        IAuthService authService, ClaimsPrincipal user, ChangePasswordRequest request)
    {
        if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return TypedResults.Unauthorized();

        try
        {
            await authService.ChangePasswordAsync(userId, request);
            return TypedResults.Ok("Password changed successfully");
        }
        catch (Exception ex) when (ex is UserNotFoundException || ex is InvalidCredentialsException)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

}

