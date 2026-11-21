// backend/src/Application/DependencyInjection.cs

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mockly.Application.Features.Auth.Services;
using Mockly.Application.Features.Interviews.Services;
using Mockly.Application.Features.Recruiter.Services;

namespace Mockly.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<IRecruiterService, RecruiterService>();

        return services;
    }
}
