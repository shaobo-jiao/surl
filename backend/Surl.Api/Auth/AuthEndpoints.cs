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
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", LogoutAsync)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/change-password", ChangePasswordAsync)
            .WithValidationFilter<ChangePasswordRequest>()
            .WithDescription("Change password for authenticated user")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        group.MapGet("/me", MeAsync)
            .WithDescription("Get the currently authenticated user")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    public static async Task<Results<Created<UserResponse>, ProblemHttpResult>> RegisterAsync(
        IAuthService authService, [FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // return Created as of now. may add a /me endpoint to get current user info in the future
        var input = new RegisterInput(request.Username, request.Password, request.Email);
        var result = await authService.RegisterAsync(input, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Created(string.Empty, result.Value!.ToResponse())
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }

    public static async Task<Results<Ok<LoginResponse>, ProblemHttpResult>> LoginAsync(
        IAuthService authService, LoginRequest request, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        var input = new LoginInput(request.Username, request.Password);
        var result = await authService.LoginAsync(input, cancellationToken);

        if (!result.IsSuccessful)
            return TypedResults.Problem(result.Error.ToProblemDetails());

        // return access token in response and set refresh token cookie
        var loginResponse = new LoginResponse(result.Value!.AccessToken);
        httpResponse.Cookies.SetRefreshToken(result.Value!.RefreshToken);
        return TypedResults.Ok(loginResponse);
    }

    public static async Task<Results<Ok<RefreshResponse>, ProblemHttpResult>> RefreshAsync(
        IAuthService authService, HttpContext httpContext, CancellationToken cancellationToken)
    {
        // get refresh token from cookie
        var refreshToken = httpContext.Request.Cookies.GetRefreshToken();
        if (refreshToken is null)
            return TypedResults.Problem(AuthError.EmptyRefreshToken.ToProblemDetails());
        
        var result = await authService.RefreshAsync(refreshToken, cancellationToken);

        if (!result.IsSuccessful)
            return TypedResults.Problem(result.Error.ToProblemDetails()); 
        
        // return new access token in response and set new refresh token cookie
        var refreshResponse = new RefreshResponse(result.Value!.AccessToken);
        httpContext.Response.Cookies.SetRefreshToken(result.Value!.RefreshToken);
        return TypedResults.Ok(refreshResponse);
    }

    public static async Task<Results<Ok<string>, ProblemHttpResult>> LogoutAsync(
        IAuthService authService, HttpContext httpContext, CancellationToken cancellationToken)
    {
        // get refresh token from cookie
        var refreshToken = httpContext.Request.Cookies.GetRefreshToken();
        if (refreshToken is null)
            return TypedResults.Problem(AuthError.EmptyRefreshToken.ToProblemDetails());
        
        var result = await authService.LogoutAsync(refreshToken, cancellationToken);

        if (!result.IsSuccessful)
            return TypedResults.Problem(result.Error.ToProblemDetails());
        
        // clear refresh token cookie
        httpContext.Response.Cookies.DeleteRefreshToken();
        return TypedResults.Ok("Logged out successfully"); 
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

    public static async Task<Results<Ok<UserResponse>, NotFound>> MeAsync(
        IUserService userService, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var loadedUser = await userService.GetUserByIdAsync(userId, cancellationToken);
        return loadedUser == null
            ? TypedResults.NotFound()
            : TypedResults.Ok(loadedUser.ToResponse());
    }
}
