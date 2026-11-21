// backend/src/WebApi/Endpoints/RecruiterEndpoints.cs
using System.Security.Claims;
using Mockly.Application.Features.Recruiter.DTOs;
using Mockly.Application.Features.Recruiter.Services;
using Mockly.Domain.Enums;
using Mockly.WebApi.Extensions;

namespace Mockly.WebApi.Endpoints;

public static class RecruiterEndpoints
{
    // Endpoints del dashboard del reclutador (Bloque 3).
    public static IEndpointRouteBuilder MapRecruiterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/recruiter")
            .WithTags("Recruiter")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Recruiter)))
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /api/v1/recruiter/openings → búsquedas del reclutador logueado
        group.MapGet("/openings", async (ClaimsPrincipal user, IRecruiterService recruiterService, CancellationToken ct) =>
        {
            var response = await recruiterService.GetMyOpeningsAsync(user.GetUserId(), ct);
            return Results.Ok(response);
        })
        .WithName("GetMyOpenings")
        .Produces<IReadOnlyList<JobOpeningSummaryResponse>>(StatusCodes.Status200OK);

        // GET /api/v1/recruiter/openings/{id}/ranking → entrevistas completadas, mejor puntaje primero
        group.MapGet("/openings/{id:guid}/ranking", async (Guid id, ClaimsPrincipal user, IRecruiterService recruiterService, CancellationToken ct) =>
        {
            var response = await recruiterService.GetRankingAsync(id, user.GetUserId(), ct);
            return Results.Ok(response);
        })
        .WithName("GetOpeningRanking")
        .Produces<IReadOnlyList<CandidateRankingResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
