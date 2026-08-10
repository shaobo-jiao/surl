using Microsoft.AspNetCore.Mvc;
using Surl.Api.Common;

namespace Surl.Api.Users;

public static class UserErrorExtensions
{
    /// <summary>
    /// Maps UserError to ProblemDetails for endpoint return use.
    /// </summary>
    public static ProblemDetails ToProblemDetails(this UserError error)
    {
        return error switch
        {
            UserError.UserNotFound => ProblemDetailsHelper.Create(
                status: StatusCodes.Status404NotFound,
                code: "user_not_found",
                title: "User not found", 
                detail: "The user is not found."),

            UserError.DuplicateUsername => ProblemDetailsHelper.Create(
                status: StatusCodes.Status400BadRequest,
                code: "user_duplicate_username",
                title: "Username already exists", 
                detail: "The username already exists in system."),

            _ => ProblemDetailsHelper.Create(
                status: StatusCodes.Status400BadRequest,
                code: "user_general_error", 
                title: "User General Error",
                detail: "There is something wrong with the user."),
        };
    }
}
