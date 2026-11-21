// Espejo de backend/src/Application/Features/Practice/DTOs/PracticeDtos.cs

import type { ProgrammingLanguage } from '@/features/interviews/languages';
import type { ChallengeDifficulty } from '@/features/interviews/types/interview.types';

export interface PracticeChallenge {
  id: string;
  title: string;
  difficulty: ChallengeDifficulty;
  language: ProgrammingLanguage;
  attempts: number;
  bestScore: number | null;
  lastAttemptId: string | null;
}
