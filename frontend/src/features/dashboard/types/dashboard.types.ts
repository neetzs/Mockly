// Espejo de backend/src/Application/Features/Recruiter/DTOs/RecruiterDtos.cs

export interface JobOpeningSummary {
  id: string;
  title: string;
  isActive: boolean;
  completedInterviews: number;
}

export interface CandidateRanking {
  sessionId: string;
  candidateId: string;
  candidateName: string;
  candidateEmail: string;
  challengeTitle: string;
  difficulty: string;
  overallScore: number;
  codeQualityScore: number;
  problemSolvingScore: number;
  architectureScore: number;
  completedAtUtc: string;
}

/** Payload del evento "InterviewCompleted" que empuja el hub de SignalR. */
export interface InterviewCompletedEvent {
  jobOpeningId: string;
  entry: CandidateRanking;
}
