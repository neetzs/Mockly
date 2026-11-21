// backend/src/WebApi/Endpoints/PracticeEndpoints.cs
using System.Security.Claims;
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Application.Features.Practice.DTOs;
using Mockly.Application.Features.Practice.Services;
using Mockly.Domain.Enums;
using Mockly.WebApi.Extensions;

namespace Mockly.WebApi.Endpoints;

public static class PracticeEndpoints
{
    // Biblioteca de práctica: retos sin búsqueda que cualquier candidato puede resolver.
    // Resolver un intento usa los endpoints de /interviews (sala, submit, certificado).
    public static IEndpointRouteBuilder MapPracticeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/practice")
            .WithTags("Practice")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Candidate)))
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /api/v1/practice/challenges → biblioteca con el progreso del candidato
        group.MapGet("/challenges", async (ClaimsPrincipal user, IPracticeService practiceService, CancellationToken ct) =>
            Results.Ok(await practiceService.GetChallengesAsync(user.GetUserId(), ct)))
        .WithName("GetPracticeChallenges")
        .Produces<IReadOnlyList<PracticeChallengeResponse>>(StatusCodes.Status200OK);

        // POST /api/v1/practice/challenges/{id}/attempts → crea (o retoma) un intento ya iniciado
        group.MapPost("/challenges/{id:guid}/attempts", async (Guid id, ClaimsPrincipal user, IPracticeService practiceService, CancellationToken ct) =>
        {
            var attempt = await practiceService.StartAttemptAsync(id, user.GetUserId(), ct);
            return Results.Created($"/api/v1/interviews/{attempt.Id}", attempt);
        })
        .WithName("StartPracticeAttempt")
        .Produces<InterviewDetailResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
