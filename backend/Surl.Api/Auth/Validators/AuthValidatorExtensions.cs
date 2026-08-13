using FluentValidation;

namespace Surl.Api.Auth;

public static class AuthValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> Username<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.NotEmpty()
            .MinimumLength(6)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, underscores, dots and hyphens");
    }

    /// <summary>
    /// strong password validation 
    /// </summary>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.NotEmpty()
              .MinimumLength(12)
              .MaximumLength(50)
              .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
              .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
              .Matches("[0-9]").WithMessage("Password must contain a number.")
              .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.")
              .Matches("^\\S+$").WithMessage("Password must not contain whitespace.");
    }

    /// <summary>
    /// weak password validator. 
    /// note: may consider passing a PasswordPolicy argument to specify password complexity level.
    /// </summary>
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.NotEmpty()
            .MinimumLength(6)
            .MaximumLength(50);
    }
}
