import { API_BASE_URL, apiRequest } from '@/features/interviews/services/interview.service';
import type {
  CandidateRanking,
  CreateJobOpeningRequest,
  InvitedCandidate,
  JobOpeningDetail,
  JobOpeningSummary,
  LibraryChallenge,
  SessionReview,
} from '../types/dashboard.types';

const RECRUITER_URL = `${API_BASE_URL}/recruiter`;

/** El hub vive fuera de /api/v1: http://localhost:5067/hubs/interviews */
export const INTERVIEW_HUB_URL = `${new URL(API_BASE_URL).origin}/hubs/interviews`;

// Mismo helper que el resto de la app: manda el JWT y convierte los ProblemDetails en ApiError.
const request = <T,>(path: string, token: string, init: RequestInit = {}) =>
  apiRequest<T>(`${RECRUITER_URL}${path}`, token, init);

export const recruiterService = {
  getOpenings: (token: string, signal?: AbortSignal) =>
    request<JobOpeningSummary[]>('/openings', token, { signal }),

  getRanking: (openingId: string, token: string, signal?: AbortSignal) =>
    request<CandidateRanking[]>(`/openings/${openingId}/ranking`, token, { signal }),

  getLibrary: (token: string, signal?: AbortSignal) =>
    request<LibraryChallenge[]>('/library', token, { signal }),

  createOpening: (body: CreateJobOpeningRequest, token: string) =>
    request<JobOpeningSummary>('/openings', token, { method: 'POST', body: JSON.stringify(body) }),

  getOpening: (openingId: string, token: string, signal?: AbortSignal) =>
    request<JobOpeningDetail>(`/openings/${openingId}`, token, { signal }),

  closeOpening: (openingId: string, token: string) =>
    request<JobOpeningDetail>(`/openings/${openingId}/close`, token, { method: 'PATCH' }),

  invite: (openingId: string, email: string, token: string) =>
    request<InvitedCandidate>(`/openings/${openingId}/invitations`, token, {
      method: 'POST',
      body: JSON.stringify({ email }),
    }),

  getSessionReview: (sessionId: string, token: string, signal?: AbortSignal) =>
    request<SessionReview>(`/sessions/${sessionId}`, token, { signal }),
};
