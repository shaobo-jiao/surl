using FluentValidation;

namespace Surl.Api.Common;

/// <summary>
/// Represents a validation filter of given type of request.
/// </summary>
/// <typeparam name="T">Type of request to validate</typeparam>
public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    private IValidator<T> _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        T model = context.Arguments.OfType<T>().First();

        var validationResults = await _validator.ValidateAsync(model, context.HttpContext.RequestAborted);
        if (!validationResults.IsValid)
            return TypedResults.ValidationProblem(validationResults.ToDictionary());

        return await next(context);
    }
}

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidationFilter<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>()
            .ProducesValidationProblem();
    }
}
