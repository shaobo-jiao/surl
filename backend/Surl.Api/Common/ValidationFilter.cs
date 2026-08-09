using System.Text.Json;
using FluentValidation;

namespace Surl.Api.Common;

/// <summary>
/// Represents a validation filter for given type of request.
/// </summary>
/// <typeparam name="T">Type of request to validate</typeparam>
public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    private IValidator<T> _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        T model = context.Arguments.OfType<T>().First();

        var result = await _validator.ValidateAsync(model, context.HttpContext.RequestAborted);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => JsonNamingPolicy.CamelCase.ConvertName(g.Key), // convert keys to camelCase
                    g => g.Select(x => x.ErrorMessage).ToArray());
            return TypedResults.ValidationProblem(errors);
        }
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
