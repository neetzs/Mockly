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

        // GET /api/v1/recruiter/library → retos de la biblioteca para armar una búsqueda
        group.MapGet("/library", async (IRecruiterService recruiterService, CancellationToken ct) =>
            Results.Ok(await recruiterService.GetLibraryAsync(ct)))
        .WithName("GetChallengeLibrary")
        .Produces<IReadOnlyList<LibraryChallengeResponse>>(StatusCodes.Status200OK);

        // POST /api/v1/recruiter/openings → crea una búsqueda con copias de los retos elegidos
        group.MapPost("/openings", async (CreateJobOpeningRequest request, ClaimsPrincipal user, IRecruiterService recruiterService, CancellationToken ct) =>
        {
            var opening = await recruiterService.CreateOpeningAsync(user.GetUserId(), request, ct);
            return Results.Created($"/api/v1/recruiter/openings/{opening.Id}", opening);
        })
        .WithName("CreateOpening")
        .Produces<JobOpeningSummaryResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/recruiter/openings/{id} → retos de la búsqueda y avance de cada candidato invitado
        group.MapGet("/openings/{id:guid}", async (Guid id, ClaimsPrincipal user, IRecruiterService recruiterService, CancellationToken ct) =>
            Results.Ok(await recruiterService.GetOpeningAsync(id, user.GetUserId(), ct)))
        .WithName("GetOpening")
        .Produces<JobOpeningDetailResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PATCH /api/v1/recruiter/openings/{id}/close → cierra la búsqueda (no admite más invitaciones)
        group.MapPatch("/openings/{id:guid}/close", async (Guid id, ClaimsPrincipal user, IRecruiterService recruiterService, CancellationToken ct) =>
            Results.Ok(await recruiterService.CloseOpeningAsync(id, user.GetUserId(), ct)))
        .WithName("CloseOpening")
        .Produces<JobOpeningDetailResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // POST /api/v1/recruiter/openings/{id}/invitations → invita a un candidato registrado por email
        group.MapPost("/openings/{id:guid}/invitations", async (Guid id, InviteCandidateRequest request, ClaimsPrincipal user, IRecruiterService recruiterService, CancellationToken ct) =>
        {
            var invited = await recruiterService.InviteCandidateAsync(id, user.GetUserId(), request, ct);
            return Results.Created($"/api/v1/recruiter/openings/{id}", invited);
        })
        .WithName("InviteCandidate")
        .Produces<InvitedCandidateResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // GET /api/v1/recruiter/sessions/{id} → código enviado y devolución completa de la IA
        group.MapGet("/sessions/{id:guid}", async (Guid id, ClaimsPrincipal user, IRecruiterService recruiterService, CancellationToken ct) =>
            Results.Ok(await recruiterService.GetSessionReviewAsync(id, user.GetUserId(), ct)))
        .WithName("GetSessionReview")
        .Produces<SessionReviewResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
