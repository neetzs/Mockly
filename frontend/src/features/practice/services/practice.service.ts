import { API_BASE_URL, apiRequest } from '@/features/interviews/services/interview.service';
import type { InterviewDetail } from '@/features/interviews/types/interview.types';
import type { PracticeChallenge } from '../types/practice.types';

const PRACTICE_URL = `${API_BASE_URL}/practice`;

export const practiceService = {
  getChallenges: (token: string, signal?: AbortSignal) =>
    apiRequest<PracticeChallenge[]>(`${PRACTICE_URL}/challenges`, token, { signal }),

  /** Crea un intento ya iniciado (o retoma el que está en curso). Devuelve la sesión para ir a la sala. */
  startAttempt: (challengeId: string, token: string) =>
    apiRequest<InterviewDetail>(`${PRACTICE_URL}/challenges/${challengeId}/attempts`, token, { method: 'POST' }),
};
