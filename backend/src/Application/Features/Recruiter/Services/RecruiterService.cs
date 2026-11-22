// backend/src/Application/Features/Recruiter/Services/RecruiterService.cs
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Mockly.Application.Common.Exceptions;
using Mockly.Application.Common.Interfaces;
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Application.Features.Recruiter.DTOs;
using Mockly.Domain.Entities;
using Mockly.Domain.Enums;

namespace Mockly.Application.Features.Recruiter.Services;

/// Casos de uso del reclutador: sus búsquedas, el ranking, crear búsquedas, invitar candidatos y revisar entrevistas.
public sealed class RecruiterService : IRecruiterService
{
    private readonly IApplicationDbContext _context;
    private readonly IValidator<CreateJobOpeningRequest> _createValidator;
    private readonly IValidator<InviteCandidateRequest> _inviteValidator;

    public RecruiterService(
        IApplicationDbContext context,
        IValidator<CreateJobOpeningRequest> createValidator,
        IValidator<InviteCandidateRequest> inviteValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _inviteValidator = inviteValidator;
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
        await EnsureOwnsOpeningAsync(jobOpeningId, recruiterId, cancellationToken);

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

    public async Task<IReadOnlyList<LibraryChallengeResponse>> GetLibraryAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TechnicalChallenges
            .AsNoTracking()
            .Where(c => c.JobOpeningId == null)
            .OrderBy(c => c.Language)
            .ThenBy(c => c.Difficulty)
            .Select(c => new LibraryChallengeResponse(c.Id, c.Title, c.Description, c.Difficulty.ToString(), c.Language.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<JobOpeningSummaryResponse> CreateOpeningAsync(Guid recruiterId, CreateJobOpeningRequest request, CancellationToken cancellationToken = default)
    {
        // Paso 1 — Validar el formato (400).
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        // Paso 2 — Traer los retos elegidos, solo de la biblioteca. Si falta alguno, el id no es válido (404).
        var libraryChallenges = await _context.TechnicalChallenges
            .Where(c => request.ChallengeIds.Contains(c.Id) && c.JobOpeningId == null)
            .ToListAsync(cancellationToken);

        if (libraryChallenges.Count != request.ChallengeIds.Count)
        {
            throw new KeyNotFoundException("Uno o más retos no existen en la biblioteca.");
        }

        // Paso 3 — Crear la búsqueda con una copia de cada reto, en el orden que eligió el reclutador.
        var opening = new JobOpening(request.Title, request.Description ?? string.Empty, recruiterId);
        foreach (var id in request.ChallengeIds)
        {
            opening.AddChallengeFromLibrary(libraryChallenges.Single(c => c.Id == id));
        }

        // Add recorre el grafo: la búsqueda y sus retos copiados quedan como nuevos (INSERT).
        _context.JobOpenings.Add(opening);
        await _context.SaveChangesAsync(cancellationToken);

        return new JobOpeningSummaryResponse(opening.Id, opening.Title, opening.IsActive, 0);
    }

    public async Task<JobOpeningDetailResponse> GetOpeningAsync(Guid jobOpeningId, Guid recruiterId, CancellationToken cancellationToken = default)
    {
        await EnsureOwnsOpeningAsync(jobOpeningId, recruiterId, cancellationToken);

        // Dos consultas en lugar de un Include de dos colecciones: un solo JOIN multiplicaría
        // filas (retos × sesiones), lo que se conoce como "explosión cartesiana".
        var opening = await _context.JobOpenings
            .AsNoTracking()
            .Include(j => j.Challenges)
            .SingleAsync(j => j.Id == jobOpeningId, cancellationToken);

        var sessions = await _context.InterviewSessions
            .AsNoTracking()
            .Include(s => s.Candidate)
            .Include(s => s.TechnicalChallenge)
            .Where(s => s.JobOpeningId == jobOpeningId)
            .ToListAsync(cancellationToken);

        // Orden en que los eligió el reclutador (las copias se crearon en ese orden).
        var challenges = opening.Challenges
            .OrderBy(c => c.CreatedAtUtc)
            .Select(c => new OpeningChallengeResponse(c.Id, c.Title, c.Difficulty.ToString(), c.Language.ToString()))
            .ToList();

        // Una tarjeta por candidato con su avance en cada reto.
        // Agrupamos por Id y no por el objeto: con AsNoTracking EF no reutiliza instancias,
        // así que el mismo candidato puede venir como dos objetos distintos.
        var candidates = sessions
            .GroupBy(s => s.CandidateId)
            .Select(g => ToInvitedCandidate(g.First().Candidate, g))
            .OrderBy(c => c.FullName)
            .ToList();

        return new JobOpeningDetailResponse(opening.Id, opening.Title, opening.Description, opening.IsActive, challenges, candidates);
    }

    public async Task<InvitedCandidateResponse> InviteCandidateAsync(Guid jobOpeningId, Guid recruiterId, InviteCandidateRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _inviteValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        await EnsureOwnsOpeningAsync(jobOpeningId, recruiterId, cancellationToken);

        // Mismo criterio que el registro: los emails se guardan normalizados en minúsculas.
        var email = request.Email.Trim().ToLowerInvariant();
        var candidate = await _context.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"No hay ningún usuario registrado con {email}. Pedile que cree su cuenta en Mockly y volvé a invitarlo.");

        var opening = await _context.JobOpenings
            .Include(j => j.Challenges)
            .Include(j => j.InterviewSessions) // para detectar si ya estaba invitado
            .SingleAsync(j => j.Id == jobOpeningId, cancellationToken);

        // Las reglas (rol, duplicado, búsqueda cerrada) viven en el dominio.
        var sessions = opening.Invite(candidate);

        // Explícito a propósito: las sesiones ya tienen Id (Guid generado en el código) y, si EF las
        // descubriera solo por la colección, las trataría como existentes y haría UPDATE en vez de INSERT.
        _context.InterviewSessions.AddRange(sessions);
        await _context.SaveChangesAsync(cancellationToken);

        return new InvitedCandidateResponse(
            candidate.Id,
            candidate.FullName,
            candidate.Email,
            sessions
                .Select(s => new CandidateSessionResponse(
                    s.Id,
                    opening.Challenges.Single(c => c.Id == s.TechnicalChallengeId).Title,
                    s.Status.ToString(),
                    s.OverallScore))
                .ToList());
    }

    public async Task<SessionReviewResponse> GetSessionReviewAsync(Guid sessionId, Guid recruiterId, CancellationToken cancellationToken = default)
    {
        var session = await _context.InterviewSessions
            .AsNoTracking()
            .Include(s => s.Candidate)
            .Include(s => s.TechnicalChallenge)
            .Include(s => s.JobOpening)
            .SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new KeyNotFoundException("Entrevista no encontrada.");

        // Las prácticas no tienen búsqueda: ningún reclutador puede verlas.
        if (session.JobOpening is not { } opening || opening.RecruiterId != recruiterId)
        {
            throw new ForbiddenAccessException("Esta entrevista no pertenece a una de tus búsquedas.");
        }

        return new SessionReviewResponse(
            session.Id,
            opening.Id,
            opening.Title,
            session.Candidate.FullName,
            session.Candidate.Email,
            session.TechnicalChallenge.Title,
            session.TechnicalChallenge.Description,
            session.TechnicalChallenge.Difficulty.ToString(),
            session.TechnicalChallenge.Language.ToString(),
            session.Status.ToString(),
            string.IsNullOrEmpty(session.SubmittedCode) ? null : session.SubmittedCode,
            EvaluationResponse.FromJson(session.AiFeedbackJson),
            session.CompletedAtUtc);
    }

    /// La búsqueda tiene que existir (404) y ser de este reclutador (403).
    private async Task EnsureOwnsOpeningAsync(Guid jobOpeningId, Guid recruiterId, CancellationToken cancellationToken)
    {
        var ownerId = await _context.JobOpenings
            .Where(j => j.Id == jobOpeningId)
            .Select(j => (Guid?)j.RecruiterId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Búsqueda laboral no encontrada.");

        if (ownerId != recruiterId)
        {
            throw new ForbiddenAccessException("Esta búsqueda laboral no te pertenece.");
        }
    }

    private static InvitedCandidateResponse ToInvitedCandidate(User candidate, IEnumerable<InterviewSession> sessions) =>
        new(
            candidate.Id,
            candidate.FullName,
            candidate.Email,
            sessions
                .OrderBy(s => s.TechnicalChallenge.CreatedAtUtc)
                .Select(s => new CandidateSessionResponse(s.Id, s.TechnicalChallenge.Title, s.Status.ToString(), s.OverallScore))
                .ToList());
}
