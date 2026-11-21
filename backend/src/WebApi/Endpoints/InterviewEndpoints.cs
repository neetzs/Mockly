// backend/src/WebApi/Endpoints/InterviewEndpoints.cs
using System.Security.Claims;
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Application.Features.Interviews.Services;
using Mockly.Domain.Enums;
using Mockly.WebApi.Extensions;
using Mockly.WebApi.RateLimiting;

namespace Mockly.WebApi.Endpoints;

public static class InterviewEndpoints
{
    // Define los endpoints de la sala de entrevista (Bloque 2).
    public static IEndpointRouteBuilder MapInterviewEndpoints(this IEndpointRouteBuilder app)
    {
        // Todo el grupo exige un JWT válido con rol Candidate (el claim Role del token).
        // Un Recruiter recibe 403: su vista será el dashboard, no la sala.
        var group = app.MapGroup("/api/v1/interviews")
            .WithTags("Interviews")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Candidate)))
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/interviews/me → entrevistas asignadas al candidato logueado
        // (se declara antes que /{id:guid}; igual no chocan porque "me" no es un Guid)
        group.MapGet("/me", async (ClaimsPrincipal user, IInterviewService interviewService, CancellationToken ct) =>
        {
            var response = await interviewService.GetMyInterviewsAsync(user.GetUserId(), ct);
            return Results.Ok(response);
        })
        .WithName("GetMyInterviews")
        .Produces<IReadOnlyList<InterviewSummaryResponse>>(StatusCodes.Status200OK);

        // GET /api/v1/interviews/{id} → carga el reto para el Monaco Editor (no cambia el estado)
        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IInterviewService interviewService, CancellationToken ct) =>
        {
            var response = await interviewService.GetInterviewAsync(id, user.GetUserId(), ct);
            return Results.Ok(response);
        })
        .WithName("GetInterview")
        .Produces<InterviewDetailResponse>(StatusCodes.Status200OK);

        // POST /api/v1/interviews/{id}/start → NotStarted → InProgress (idempotente si ya está en curso)
        group.MapPost("/{id:guid}/start", async (Guid id, ClaimsPrincipal user, IInterviewService interviewService, CancellationToken ct) =>
        {
            var response = await interviewService.StartInterviewAsync(id, user.GetUserId(), ct);
            return Results.Ok(response);
        })
        .WithName("StartInterview")
        .Produces<InterviewDetailResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // GET /api/v1/interviews/{id}/certificate → PDF del certificado (solo entrevistas completadas)
        group.MapGet("/{id:guid}/certificate", async (Guid id, ClaimsPrincipal user, IInterviewService interviewService, CancellationToken ct) =>
        {
            var certificate = await interviewService.GetCertificateAsync(id, user.GetUserId(), ct);

            // Results.File arma la respuesta binaria con Content-Type application/pdf
            // y Content-Disposition: attachment (el navegador lo trata como descarga).
            return Results.File(certificate.Content, "application/pdf", certificate.FileName);
        })
        .WithName("GetCertificate")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
        .ProducesProblem(StatusCodes.Status409Conflict);

        // POST /api/v1/interviews/{id}/submit → envía el código y devuelve la evaluación
        group.MapPost("/{id:guid}/submit", async (Guid id, SubmitCodeRequest request, ClaimsPrincipal user, IInterviewService interviewService, CancellationToken ct) =>
        {
            var response = await interviewService.SubmitCodeAsync(id, user.GetUserId(), request, ct);
            return Results.Ok(response);
        })
        .WithName("SubmitCode")
        .RequireRateLimiting(RateLimitPolicies.AiEvaluation) // cada envío cuesta tokens de la IA
        .Produces<EvaluationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
