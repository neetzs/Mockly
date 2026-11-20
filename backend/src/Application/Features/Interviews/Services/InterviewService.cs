// backend/src/Application/Features/Interviews/Services/InterviewService.cs
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Mockly.Application.Common.Interfaces;
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Domain.Interfaces;
using System.Text.Json;

namespace Mockly.Application.Features.Interviews.Services;

/// Orquesta la carga del reto y la evaluación del código enviado por el candidato.
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

    public async Task<InterviewDetailResponse> GetInterviewAsync(Guid interviewId, CancellationToken cancellationToken = default)
    {
        var session = await _context.InterviewSessions
            .Include(s => s.TechnicalChallenge)
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == interviewId, cancellationToken);

        if (session is null)
        {
            throw new KeyNotFoundException("Entrevista no encontrada.");
        }

        return new InterviewDetailResponse(
            session.Id,
            session.Status.ToString(),
            session.TechnicalChallenge.Title,
            session.TechnicalChallenge.Description,
            session.TechnicalChallenge.InitialCodeTemplate,
            session.TechnicalChallenge.Difficulty.ToString());
    }

    public async Task<EvaluationResponse> SubmitCodeAsync(Guid interviewId, SubmitCodeRequest request, CancellationToken cancellationToken = default)
    {
        // Paso 1 — Validar 
        var validation = await _submitValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        // Paso 2 — Buscar la sesión junto con su reto
        var session = await _context.InterviewSessions
            .Include(s => s.TechnicalChallenge)
            .SingleOrDefaultAsync(s => s.Id == interviewId, cancellationToken);

        if (session is null)
        {
            throw new KeyNotFoundException("Entrevista no encontrada.");
        }

        // Paso 3 — Pedirle a la IA que evalúe el código
        var result = await _evaluationService.EvaluateSubmissionAsync(
            session.TechnicalChallenge.Description,
            request.Code,
            cancellationToken);

        // Paso 4 — Guardar el resultado en la sesión
        var feedbackJson = JsonSerializer.Serialize(result);
        session.Complete(request.Code, result.OverallScore, feedbackJson);
        await _context.SaveChangesAsync(cancellationToken);

        // Paso 5 — Devolver el DTO a la API
        return new EvaluationResponse(
            result.OverallScore,
            result.CodeQualityScore,
            result.ProblemSolvingScore,
            result.ArchitectureScore,
            result.Summary,
            result.Strengths,
            result.Improvements);
    }
}
