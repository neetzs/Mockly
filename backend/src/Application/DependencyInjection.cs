// backend/src/Application/DependencyInjection.cs

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mockly.Application.Features.Auth.Services;

namespace Mockly.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}