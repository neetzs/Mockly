// backend/src/Application/Features/Practice/Services/PracticeService.cs
using Microsoft.EntityFrameworkCore;
using Mockly.Application.Common.Interfaces;
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Application.Features.Interviews.Services;
using Mockly.Application.Features.Practice.DTOs;
using Mockly.Domain.Entities;
using Mockly.Domain.Enums;
using Mockly.Domain.Exceptions;

namespace Mockly.Application.Features.Practice.Services;

/// Modo práctica: cualquier candidato puede resolver los retos de la biblioteca (JobOpeningId = null)
/// las veces que quiera. Cada intento es una InterviewSession sin búsqueda, así reutilizamos la sala,
/// el motor de IA, el rate limit y el certificado sin duplicar código.
public sealed class PracticeService : IPracticeService
{
    // Tope de intentos abiertos a la vez: evita llenar la base con sesiones vacías tocando "Practicar".
    private const int MaxOpenAttempts = 3;

    private readonly IApplicationDbContext _context;
    private readonly IInterviewService _interviewService;

    public PracticeService(IApplicationDbContext context, IInterviewService interviewService)
    {
        _context = context;
        _interviewService = interviewService;
    }

    public async Task<IReadOnlyList<PracticeChallengeResponse>> GetChallengesAsync(Guid candidateId, CancellationToken cancellationToken = default)
    {
        // Una sola consulta: por cada reto de la biblioteca, el resumen de los intentos de ESTE candidato.
        // EF traduce cada Count/Max/FirstOrDefault a una subconsulta SQL correlacionada.
        var challenges = await _context.TechnicalChallenges
            .AsNoTracking()
            .Where(c => c.JobOpeningId == null)
            .OrderBy(c => c.Language)
            .ThenBy(c => c.Difficulty)
            .Select(c => new PracticeChallengeResponse(
                c.Id,
                c.Title,
                c.Difficulty.ToString(),
                c.Language.ToString(),
                _context.InterviewSessions.Count(s => s.CandidateId == candidateId && s.TechnicalChallengeId == c.Id),
                _context.InterviewSessions
                    .Where(s => s.CandidateId == candidateId && s.TechnicalChallengeId == c.Id)
                    .Max(s => s.OverallScore),
                _context.InterviewSessions
                    .Where(s => s.CandidateId == candidateId && s.TechnicalChallengeId == c.Id)
                    .OrderByDescending(s => s.CreatedAtUtc)
                    .Select(s => (Guid?)s.Id)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return challenges;
    }

    public async Task<InterviewDetailResponse> StartAttemptAsync(Guid challengeId, Guid candidateId, CancellationToken cancellationToken = default)
    {
        var challenge = await _context.TechnicalChallenges
            .SingleOrDefaultAsync(c => c.Id == challengeId, cancellationToken)
            ?? throw new KeyNotFoundException("Reto no encontrado.");

        // Si ya hay un intento en curso de este reto, lo retomamos en vez de crear otro.
        var openAttemptId = await _context.InterviewSessions
            .Where(s => s.CandidateId == candidateId
                && s.TechnicalChallengeId == challengeId
                && s.JobOpeningId == null
                && s.Status == InterviewStatus.InProgress)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (openAttemptId is { } existingId)
        {
            return await _interviewService.GetInterviewAsync(existingId, candidateId, cancellationToken);
        }

        var openAttempts = await _context.InterviewSessions.CountAsync(
            s => s.CandidateId == candidateId && s.JobOpeningId == null && s.Status == InterviewStatus.InProgress,
            cancellationToken);
        if (openAttempts >= MaxOpenAttempts)
        {
            throw new DomainException(
                $"Tenés {openAttempts} prácticas sin terminar. Enviá alguna antes de empezar otra.");
        }

        // La regla "solo retos de la biblioteca" la valida el dominio (factory method).
        var session = InterviewSession.StartPractice(candidateId, challenge);
        _context.InterviewSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return await _interviewService.GetInterviewAsync(session.Id, candidateId, cancellationToken);
    }
}
