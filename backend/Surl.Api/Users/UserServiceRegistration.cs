using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Surl.Api.Users;

public static class UserServiceRegistration
{
    /// <summary>
    /// Registers all services required by the User module
    /// </summary>
    /// <param name="services"></param>
    public static void AddUserServices(this IServiceCollection services)
    {
        // business services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // validators
        services.AddScoped<IValidator<QueryUserParams>, QueryUserParamsValidator>();
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
    }
}
