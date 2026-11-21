// backend/src/Application/Features/Recruiter/Services/IRecruiterService.cs
using Mockly.Application.Features.Recruiter.DTOs;

namespace Mockly.Application.Features.Recruiter.Services;

public interface IRecruiterService
{
    Task<IReadOnlyList<JobOpeningSummaryResponse>> GetMyOpeningsAsync(Guid recruiterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CandidateRankingResponse>> GetRankingAsync(Guid jobOpeningId, Guid recruiterId, CancellationToken cancellationToken = default);
}
