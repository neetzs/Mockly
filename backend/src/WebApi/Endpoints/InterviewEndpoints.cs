// backend/src/WebApi/Endpoints/InterviewEndpoints.cs
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Application.Features.Interviews.Services;

namespace Mockly.WebApi.Endpoints;

public static class InterviewEndpoints
{
    // Define los endpoints de la sala de entrevista (Bloque 2).
    public static IEndpointRouteBuilder MapInterviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/interviews")
            .WithTags("Interviews");

        // GET /api/v1/interviews/{id} → carga el reto para el Monaco Editor
        group.MapGet("/{id:guid}", async (Guid id, IInterviewService interviewService, CancellationToken ct) =>
        {
            var response = await interviewService.GetInterviewAsync(id, ct);
            return Results.Ok(response);
        })
        .WithName("GetInterview")
        .RequireAuthorization()
        .Produces<InterviewDetailResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // POST /api/v1/interviews/{id}/submit → envía el código y devuelve la evaluación
        group.MapPost("/{id:guid}/submit", async (Guid id, SubmitCodeRequest request, IInterviewService interviewService, CancellationToken ct) =>
        {
            var response = await interviewService.SubmitCodeAsync(id, request, ct);
            return Results.Ok(response);
        })
        .WithName("SubmitCode")
        .RequireAuthorization()
        .Produces<EvaluationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
