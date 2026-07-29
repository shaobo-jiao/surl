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
                title: "Username already exists"),

            AuthError.InvalidCredentials => ProblemDetailsHelper.Create(
                status: StatusCodes.Status401Unauthorized,
                code: "auth_invalid_credentials",
                title: "Invalid credentials"),

            AuthError.InvalidRefreshToken => ProblemDetailsHelper.Create(
                status: StatusCodes.Status401Unauthorized,
                code: "auth_invalid_refresh_token",
                title: "Invalid refresh token"),

            AuthError.UserNotFound => ProblemDetailsHelper.Create(
                status: StatusCodes.Status400BadRequest,
                code: "auth_user_not_found",
                title: "User not found"),

            _ => ProblemDetailsHelper.Create(
                status: StatusCodes.Status400BadRequest,
                code: "auth_general_error"),
        };
    }
}
