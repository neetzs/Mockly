// backend/src/Application/Features/Practice/Services/IPracticeService.cs
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Application.Features.Practice.DTOs;

namespace Mockly.Application.Features.Practice.Services;

public interface IPracticeService
{
    Task<IReadOnlyList<PracticeChallengeResponse>> GetChallengesAsync(Guid candidateId, CancellationToken cancellationToken = default);

    /// Crea un intento nuevo (ya iniciado) del reto. Devuelve el mismo DTO que la sala de entrevista.
    Task<InterviewDetailResponse> StartAttemptAsync(Guid challengeId, Guid candidateId, CancellationToken cancellationToken = default);
}
