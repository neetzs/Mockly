// backend/src/Application/Features/Interviews/Services/IInterviewService.cs
using Mockly.Application.Features.Interviews.DTOs;

namespace Mockly.Application.Features.Interviews.Services;

/// Todas las operaciones reciben el id del usuario autenticado (claim "sub" del JWT)
/// para verificar que la entrevista le pertenece.
public interface IInterviewService
{
    Task<IReadOnlyList<InterviewSummaryResponse>> GetMyInterviewsAsync(Guid candidateId, CancellationToken cancellationToken = default);

    Task<InterviewDetailResponse> GetInterviewAsync(Guid interviewId, Guid candidateId, CancellationToken cancellationToken = default);

    Task<InterviewDetailResponse> StartInterviewAsync(Guid interviewId, Guid candidateId, CancellationToken cancellationToken = default);

    Task<EvaluationResponse> SubmitCodeAsync(Guid interviewId, Guid candidateId, SubmitCodeRequest request, CancellationToken cancellationToken = default);
}
