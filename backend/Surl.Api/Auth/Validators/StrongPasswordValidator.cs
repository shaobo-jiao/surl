using FluentValidation;

namespace Surl.Api.Auth;

public class StrongPasswordValidator : AbstractValidator<string>
  {
      public StrongPasswordValidator()
      {
          RuleFor(password => password)
              .Cascade(CascadeMode.Stop)
              .NotEmpty()
              .MinimumLength(12)
              .MaximumLength(100)
              .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
              .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
              .Matches("[0-9]").WithMessage("Password must contain a number.")
              .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.")
              .Must(password => password.All(character => !char.IsWhiteSpace(character)))
              .WithMessage("Password must not contain whitespace.");
      }
  }
