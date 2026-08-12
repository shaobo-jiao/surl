using Microsoft.AspNetCore.Mvc;
using Surl.Api.Common;

namespace Surl.Api.Auth;

public static class AuthErrorExtensions
{
    /// <summary>
    /// Maps AuthError to ProblemDetails for endpoint return use.
    /// </summary>
    public static ProblemDetails ToProblemDetails(this AuthError error)
    {
        return error switch
        {
            AuthError.DuplicateUsername => ProblemDetailsHelper.Create(
                status: StatusCodes.Status400BadRequest,
                code: "auth_duplicate_username",
                title: "Username already exists", 
                detail: "The username already exists in system."),

            AuthError.InvalidCredentials => ProblemDetailsHelper.Create(
                status: StatusCodes.Status401Unauthorized,
                code: "auth_invalid_credentials",
                title: "Invalid credentials", 
                detail: "The provided credentials are invalid."),

            AuthError.InvalidRefreshToken => ProblemDetailsHelper.Create(
                status: StatusCodes.Status401Unauthorized,
                code: "auth_invalid_refresh_token",
                title: "Invalid refresh token", 
                detail: "The provided refresh token is invalid."),

            AuthError.RefreshTokenNotFound => ProblemDetailsHelper.Create(
                status: StatusCodes.Status401Unauthorized,
                code: "auth_refresh_token_not_found",
                title: "Refresh Token Not Found", 
                detail: "The provided refresh token is not found."),
            
            AuthError.EmptyRefreshToken => ProblemDetailsHelper.Create(
                status: StatusCodes.Status401Unauthorized,
                code: "auth_empty_refresh_token",
                title: "Empty Refresh Token", 
                detail: "No refresh token is provided."),

            AuthError.UserNotFound => ProblemDetailsHelper.Create(
                status: StatusCodes.Status400BadRequest,
                code: "auth_user_not_found",
                title: "User not found", 
                detail: "The user is not found in system."),

            _ => ProblemDetailsHelper.Create(
                status: StatusCodes.Status400BadRequest,
                code: "auth_general_error", 
                title: "Auth General Error", 
                detail: "There is something wrong with the authentication."),
        };
    }
}
