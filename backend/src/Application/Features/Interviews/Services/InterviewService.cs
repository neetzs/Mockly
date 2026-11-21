// backend/src/Application/Features/Interviews/Services/InterviewService.cs
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Mockly.Application.Common.Exceptions;
using Mockly.Application.Common.Interfaces;
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Application.Features.Recruiter.DTOs;
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
    private readonly IInterviewNotifier _notifier;

    public InterviewService(
        IApplicationDbContext context,
        IValidator<SubmitCodeRequest> submitValidator,
        IInterviewEvaluationService evaluationService,
        IInterviewNotifier notifier)
    {
        _context = context;
        _submitValidator = submitValidator;
        _evaluationService = evaluationService;
        _notifier = notifier;
    }

    public async Task<IReadOnlyList<InterviewSummaryResponse>> GetMyInterviewsAsync(Guid candidateId, CancellationToken cancellationToken = default)
    {
        // Proyección con Select: EF arma un SELECT solo con las columnas necesarias
        // (no trae las entidades completas ni el código enviado).
        return await _context.InterviewSessions
            .AsNoTracking()
            .Where(s => s.CandidateId == candidateId)
            .OrderBy(s => s.TechnicalChallenge.Difficulty)
            .Select(s => new InterviewSummaryResponse(
                s.Id,
                s.Status.ToString(),
                s.TechnicalChallenge.Title,
                s.TechnicalChallenge.Difficulty.ToString(),
                s.OverallScore,
                s.CompletedAtUtc))
            .ToListAsync(cancellationToken);
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

        // Paso 6 — Avisar en tiempo real al reclutador dueño de la búsqueda.
        // Va DESPUÉS de guardar: nunca anunciamos algo que no quedó persistido.
        // Usamos CancellationToken.None: si el candidato cerró la pestaña justo ahora,
        // el resultado ya se guardó y el reclutador igual tiene que enterarse.
        await _notifier.NotifyInterviewCompletedAsync(
            session.JobOpening.RecruiterId,
            new InterviewCompletedEvent(session.JobOpeningId, CandidateRankingResponse.FromSession(session)),
            CancellationToken.None);

        // Paso 7 — Devolver el DTO a la API.
        return ToEvaluationResponse(result);
    }

    /// Busca la sesión con su reto y verifica la pertenencia.
    /// - No existe        → 404 (KeyNotFoundException)
    /// - Es de otro usuario → 403 (ForbiddenAccessException)
    private async Task<InterviewSession> FindOwnedSessionAsync(Guid interviewId, Guid candidateId, bool tracking, CancellationToken cancellationToken)
    {
        // Candidate y JobOpening se usan para armar la notificación al reclutador.
        IQueryable<InterviewSession> query = _context.InterviewSessions
            .Include(s => s.TechnicalChallenge)
            .Include(s => s.Candidate)
            .Include(s => s.JobOpening);

        var session = await (tracking ? query : query.AsNoTracking())
            .SingleOrDefaultAsync(s => s.Id == interviewId, cancellationToken)
            ?? throw new KeyNotFoundException("Entrevista no encontrada.");

        if (session.CandidateId != candidateId)
        {
            throw new ForbiddenAccessException("Esta entrevista no está asignada a tu usuario.");
        }

        return session;
    }

    private static InterviewDetailResponse ToDetailResponse(InterviewSession session)
    {
        // Si ya fue evaluada, reconstruimos el resultado desde el JSON guardado,
        // así el frontend puede mostrarlo al recargar la sala.
        var evaluation = session.AiFeedbackJson is { } json
            ? JsonSerializer.Deserialize<TechnicalScoreResult>(json) is { } stored ? ToEvaluationResponse(stored) : null
            : null;

        return new(
            session.Id,
            session.Status.ToString(),
            session.TechnicalChallenge.Title,
            session.TechnicalChallenge.Description,
            session.TechnicalChallenge.InitialCodeTemplate,
            session.TechnicalChallenge.Difficulty.ToString(),
            string.IsNullOrEmpty(session.SubmittedCode) ? null : session.SubmittedCode,
            evaluation);
    }

    private static EvaluationResponse ToEvaluationResponse(TechnicalScoreResult result) =>
        new(
            result.OverallScore,
            result.CodeQualityScore,
            result.ProblemSolvingScore,
            result.ArchitectureScore,
            result.Summary,
            result.Strengths,
            result.Improvements);
}
