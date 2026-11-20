// backend/src/Application/Features/Interviews/Services/IInterviewService.cs
using Mockly.Application.Features.Interviews.DTOs;

namespace Mockly.Application.Features.Interviews.Services;

public interface IInterviewService
{
    Task<InterviewDetailResponse> GetInterviewAsync(Guid interviewId, CancellationToken cancellationToken = default);

    Task<EvaluationResponse> SubmitCodeAsync(Guid interviewId, SubmitCodeRequest request, CancellationToken cancellationToken = default);
}
