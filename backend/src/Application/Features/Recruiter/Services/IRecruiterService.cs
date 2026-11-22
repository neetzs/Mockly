// backend/src/Application/Features/Recruiter/Services/IRecruiterService.cs
using Mockly.Application.Features.Recruiter.DTOs;

namespace Mockly.Application.Features.Recruiter.Services;

public interface IRecruiterService
{
    Task<IReadOnlyList<JobOpeningSummaryResponse>> GetMyOpeningsAsync(Guid recruiterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CandidateRankingResponse>> GetRankingAsync(Guid jobOpeningId, Guid recruiterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LibraryChallengeResponse>> GetLibraryAsync(CancellationToken cancellationToken = default);

    Task<JobOpeningSummaryResponse> CreateOpeningAsync(Guid recruiterId, CreateJobOpeningRequest request, CancellationToken cancellationToken = default);

    Task<JobOpeningDetailResponse> GetOpeningAsync(Guid jobOpeningId, Guid recruiterId, CancellationToken cancellationToken = default);

    Task<InvitedCandidateResponse> InviteCandidateAsync(Guid jobOpeningId, Guid recruiterId, InviteCandidateRequest request, CancellationToken cancellationToken = default);

    Task<SessionReviewResponse> GetSessionReviewAsync(Guid sessionId, Guid recruiterId, CancellationToken cancellationToken = default);
}
