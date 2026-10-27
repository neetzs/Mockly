// backend/src/WebApi/Endpoints/AuthEndpoints.cs
using System.Security.Claims;
using Mockly.Application.Features.Auth.DTOs;
using Mockly.Application.Features.Auth.Services;

namespace Mockly.WebApi.Endpoints;

public static class AuthEndpoints
{
    // Define los endpoints de autenticación y autorización para la API.
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        // Endpoint para registrar un nuevo usuario.
        group.MapPost("/register", async (RegisterRequest req, IAuthService authService, CancellationToken ct) =>
        {
            var response = await authService.RegisterAsync(req, ct);
            return Results.Created($"/api/v1/users/{response.Id}", response);
        })
        .WithName("Register")
        .Produces<AuthResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // Endpoint para iniciar sesión y obtener un token JWT.
        group.MapPost("/login", async (LoginRequest req, IAuthService authService, CancellationToken ct) =>
        {
            var response = await authService.LoginAsync(req, ct);
            return Results.Ok(response);
        })
        .WithName("Login")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // Endpoint para obtener información del usuario actualmente autenticado
        group.MapGet("/me", async (ClaimsPrincipal user, IAuthService authService, CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? user.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var profile = await authService.GetCurrentUserAsync(userId, ct);
            return Results.Ok(profile);
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}