// Espejo de backend/src/Application/Features/Recruiter/DTOs/RecruiterDtos.cs

import type { ProgrammingLanguage } from '@/features/interviews/languages';
import type {
  ChallengeDifficulty,
  EvaluationResult,
  InterviewStatus,
} from '@/features/interviews/types/interview.types';

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
  language: string;
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

export interface LibraryChallenge {
  id: string;
  title: string;
  description: string;
  difficulty: ChallengeDifficulty;
  language: ProgrammingLanguage;
}

export interface CreateJobOpeningRequest {
  title: string;
  description: string;
  challengeIds: string[];
}

export interface CandidateSession {
  sessionId: string;
  challengeTitle: string;
  status: InterviewStatus;
  overallScore: number | null;
}

export interface InvitedCandidate {
  candidateId: string;
  fullName: string;
  email: string;
  sessions: CandidateSession[];
}

export interface JobOpeningDetail {
  id: string;
  title: string;
  description: string;
  isActive: boolean;
  challenges: { id: string; title: string; difficulty: ChallengeDifficulty; language: ProgrammingLanguage }[];
  candidates: InvitedCandidate[];
}

/** Código enviado por el candidato y la devolución completa de la IA. */
export interface SessionReview {
  sessionId: string;
  jobOpeningId: string;
  jobOpeningTitle: string;
  candidateName: string;
  candidateEmail: string;
  challengeTitle: string;
  challengeDescription: string;
  difficulty: ChallengeDifficulty;
  language: ProgrammingLanguage;
  status: InterviewStatus;
  submittedCode: string | null;
  evaluation: EvaluationResult | null;
  completedAtUtc: string | null;
}
