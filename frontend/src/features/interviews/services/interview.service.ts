import type { EvaluationResult, InterviewDetail, InterviewSummary } from '../types/interview.types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5067/api/v1';
const INTERVIEWS_URL = `${BASE_URL}/interviews`;

/** Error HTTP con el status, para que la UI distinga 401 (sesión vencida), 409 (estado), 503 (IA caída)... */
export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

/** fetch con el JWT y manejo de ProblemDetails (el formato de error que devuelve ApiExceptionHandler). */
async function request<T>(path: string, token: string, init: RequestInit = {}): Promise<T> {
  const res = await fetch(`${INTERVIEWS_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
      ...init.headers,
    },
  });

  if (!res.ok) {
    const problem = await res.json().catch(() => ({}));
    const fallback = res.status === 401 ? 'Tu sesión expiró. Volvé a iniciar sesión.' : 'Ocurrió un error inesperado.';
    throw new ApiError(res.status, problem.detail || problem.title || fallback);
  }

  return res.json() as Promise<T>;
}

export const interviewService = {
  getMine: (token: string, signal?: AbortSignal) =>
    request<InterviewSummary[]>('/me', token, { signal }),

  getById: (id: string, token: string, signal?: AbortSignal) =>
    request<InterviewDetail>(`/${id}`, token, { signal }),

  start: (id: string, token: string) =>
    request<InterviewDetail>(`/${id}/start`, token, { method: 'POST' }),

  submit: (id: string, code: string, token: string) =>
    request<EvaluationResult>(`/${id}/submit`, token, {
      method: 'POST',
      body: JSON.stringify({ code }),
    }),
};
