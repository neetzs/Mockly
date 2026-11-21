import type { CandidateRanking, JobOpeningSummary } from '../types/dashboard.types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5067/api/v1';
const RECRUITER_URL = `${BASE_URL}/recruiter`;

/** El hub vive fuera de /api/v1: http://localhost:5067/hubs/interviews */
export const INTERVIEW_HUB_URL = `${new URL(BASE_URL).origin}/hubs/interviews`;

async function request<T>(path: string, token: string, signal?: AbortSignal): Promise<T> {
  const res = await fetch(`${RECRUITER_URL}${path}`, {
    headers: { Authorization: `Bearer ${token}` },
    signal,
  });

  if (!res.ok) {
    const problem = await res.json().catch(() => ({}));
    throw new Error(problem.detail || problem.title || 'No se pudieron cargar los datos del dashboard.');
  }

  return res.json() as Promise<T>;
}

export const recruiterService = {
  getOpenings: (token: string, signal?: AbortSignal) =>
    request<JobOpeningSummary[]>('/openings', token, signal),

  getRanking: (openingId: string, token: string, signal?: AbortSignal) =>
    request<CandidateRanking[]>(`/openings/${openingId}/ranking`, token, signal),
};
