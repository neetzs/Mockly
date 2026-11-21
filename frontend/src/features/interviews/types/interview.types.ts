// Espejo de los DTOs de backend/src/Application/Features/Interviews/DTOs/InterviewDtos.cs
// (ASP.NET serializa en camelCase, por eso las propiedades empiezan en minúscula).

import type { ProgrammingLanguage } from '../languages';

export type InterviewStatus = 'NotStarted' | 'InProgress' | 'Completed' | 'Failed';
export type ChallengeDifficulty = 'Junior' | 'MidLevel' | 'Senior';

export interface EvaluationResult {
  overallScore: number;
  codeQualityScore: number;
  problemSolvingScore: number;
  architectureScore: number;
  summary: string;
  strengths: string[];
  improvements: string[];
}

export interface InterviewDetail {
  id: string;
  status: InterviewStatus;
  challengeTitle: string;
  challengeDescription: string;
  initialCodeTemplate: string;
  difficulty: ChallengeDifficulty;
  language: ProgrammingLanguage;
  submittedCode: string | null;
  evaluation: EvaluationResult | null;
}

export interface InterviewSummary {
  id: string;
  status: InterviewStatus;
  challengeTitle: string;
  difficulty: ChallengeDifficulty;
  language: ProgrammingLanguage;
  overallScore: number | null;
  completedAtUtc: string | null;
}
