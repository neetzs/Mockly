// backend/src/Application/Features/Recruiter/Services/RecruiterService.cs
using Microsoft.EntityFrameworkCore;
using Mockly.Application.Common.Exceptions;
using Mockly.Application.Common.Interfaces;
using Mockly.Application.Features.Recruiter.DTOs;
using Mockly.Domain.Enums;

namespace Mockly.Application.Features.Recruiter.Services;

/// Consultas del dashboard del reclutador: sus búsquedas y el ranking de cada una.
public sealed class RecruiterService : IRecruiterService
{
    private readonly IApplicationDbContext _context;

    public RecruiterService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<JobOpeningSummaryResponse>> GetMyOpeningsAsync(Guid recruiterId, CancellationToken cancellationToken = default)
    {
        return await _context.JobOpenings
            .AsNoTracking()
            .Where(j => j.RecruiterId == recruiterId)
            .OrderByDescending(j => j.IsActive)
            .ThenBy(j => j.Title)
            .Select(j => new JobOpeningSummaryResponse(
                j.Id,
                j.Title,
                j.IsActive,
                j.InterviewSessions.Count(s => s.Status == InterviewStatus.Completed)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CandidateRankingResponse>> GetRankingAsync(Guid jobOpeningId, Guid recruiterId, CancellationToken cancellationToken = default)
    {
        // Paso 1 — Verificar que la búsqueda exista y sea de este reclutador (404 / 403).
        var ownerId = await _context.JobOpenings
            .Where(j => j.Id == jobOpeningId)
            .Select(j => (Guid?)j.RecruiterId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Búsqueda laboral no encontrada.");

        if (ownerId != recruiterId)
        {
            throw new ForbiddenAccessException("Esta búsqueda laboral no te pertenece.");
        }

        // Paso 2 — Traer las entrevistas completadas, mejor puntaje primero.
        var sessions = await _context.InterviewSessions
            .AsNoTracking()
            .Include(s => s.Candidate)
            .Include(s => s.TechnicalChallenge)
            .Where(s => s.JobOpeningId == jobOpeningId && s.Status == InterviewStatus.Completed)
            .OrderByDescending(s => s.OverallScore)
            .ThenBy(s => s.CompletedAtUtc)
            .ToListAsync(cancellationToken);

        // Paso 3 — Mapear en memoria: el desglose está en un JSON que SQL no puede leer como columnas.
        return sessions.Select(CandidateRankingResponse.FromSession).ToList();
    }
}
