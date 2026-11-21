// backend/src/Application/Features/Interviews/Services/InterviewService.cs
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Mockly.Application.Common.Exceptions;
using Mockly.Application.Common.Interfaces;
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Domain.Entities;
using Mockly.Domain.Interfaces;
using System.Text.Json;

namespace Mockly.Application.Features.Interviews.Services;

/// Orquesta la carga del reto, el inicio de la sesión y la evaluación del código enviado.
public sealed class InterviewService : IInterviewService
{
    private readonly IApplicationDbContext _context;
    private readonly IValidator<SubmitCodeRequest> _submitValidator;
    private readonly IInterviewEvaluationService _evaluationService;

    public InterviewService(
        IApplicationDbContext context,
        IValidator<SubmitCodeRequest> submitValidator,
        IInterviewEvaluationService evaluationService)
    {
        _context = context;
        _submitValidator = submitValidator;
        _evaluationService = evaluationService;
    }

    public async Task<InterviewDetailResponse> GetInterviewAsync(Guid interviewId, Guid candidateId, CancellationToken cancellationToken = default)
    {
        // Solo lectura: AsNoTracking evita que EF siga los cambios de la entidad (más liviano).
        var session = await FindOwnedSessionAsync(interviewId, candidateId, tracking: false, cancellationToken);
        return ToDetailResponse(session);
    }

    public async Task<InterviewDetailResponse> StartInterviewAsync(Guid interviewId, Guid candidateId, CancellationToken cancellationToken = default)
    {
        var session = await FindOwnedSessionAsync(interviewId, candidateId, tracking: true, cancellationToken);

        // La regla (NotStarted → InProgress) vive en la entidad; acá solo la invocamos.
        session.Start();
        await _context.SaveChangesAsync(cancellationToken);

        return ToDetailResponse(session);
    }

    public async Task<EvaluationResponse> SubmitCodeAsync(Guid interviewId, Guid candidateId, SubmitCodeRequest request, CancellationToken cancellationToken = default)
    {
        // Paso 1 — Validar el formato del request (vacío, longitud).
        var validation = await _submitValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        // Paso 2 — Buscar la sesión verificando que sea del candidato logueado.
        var session = await FindOwnedSessionAsync(interviewId, candidateId, tracking: true, cancellationToken);

        // Paso 3 — Chequear el estado ANTES de llamar a la IA: si la entrevista no está
        // en curso, no tiene sentido pagar (tiempo y tokens) una evaluación que se va a descartar.
        session.EnsureInProgress();

        // Paso 4 — Pedirle a la IA que evalúe el código.
        // Si la IA falla, NO marcamos la sesión como Failed: es un error nuestro (de infraestructura),
        // no del candidato. La sesión queda InProgress y puede reintentar el envío.
        var result = await _evaluationService.EvaluateSubmissionAsync(
            session.TechnicalChallenge.Description,
            request.Code,
            cancellationToken);

        // Paso 5 — Guardar el resultado (Complete vuelve a validar el estado por seguridad).
        var feedbackJson = JsonSerializer.Serialize(result);
        session.Complete(request.Code, result.OverallScore, feedbackJson);
        await _context.SaveChangesAsync(cancellationToken);

        // Paso 6 — Devolver el DTO a la API.
        return new EvaluationResponse(
            result.OverallScore,
            result.CodeQualityScore,
            result.ProblemSolvingScore,
            result.ArchitectureScore,
            result.Summary,
            result.Strengths,
            result.Improvements);
    }

    /// Busca la sesión con su reto y verifica la pertenencia.
    /// - No existe        → 404 (KeyNotFoundException)
    /// - Es de otro usuario → 403 (ForbiddenAccessException)
    private async Task<InterviewSession> FindOwnedSessionAsync(Guid interviewId, Guid candidateId, bool tracking, CancellationToken cancellationToken)
    {
        var query = _context.InterviewSessions.Include(s => s.TechnicalChallenge);

        var session = await (tracking ? query : query.AsNoTracking())
            .SingleOrDefaultAsync(s => s.Id == interviewId, cancellationToken)
            ?? throw new KeyNotFoundException("Entrevista no encontrada.");

        if (session.CandidateId != candidateId)
        {
            throw new ForbiddenAccessException("Esta entrevista no está asignada a tu usuario.");
        }

        return session;
    }

    private static InterviewDetailResponse ToDetailResponse(InterviewSession session) =>
        new(
            session.Id,
            session.Status.ToString(),
            session.TechnicalChallenge.Title,
            session.TechnicalChallenge.Description,
            session.TechnicalChallenge.InitialCodeTemplate,
            session.TechnicalChallenge.Difficulty.ToString());
}
