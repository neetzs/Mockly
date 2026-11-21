import type { EvaluationResult, InterviewDetail, InterviewSummary } from '../types/interview.types';

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5067/api/v1';
const INTERVIEWS_URL = `${API_BASE_URL}/interviews`;

/** Error HTTP con el status, para que la UI distinga 401 (sesión vencida), 409 (estado), 503 (IA caída)... */
export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

/** fetch con el JWT y manejo de ProblemDetails (el formato de error que devuelve ApiExceptionHandler).
 *  Se exporta para que otras features (práctica) reutilicen el mismo manejo de errores. */
export async function apiRequest<T>(url: string, token: string, init: RequestInit = {}): Promise<T> {
  const res = await fetch(url, {
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

const request = <T,>(path: string, token: string, init: RequestInit = {}) =>
  apiRequest<T>(`${INTERVIEWS_URL}${path}`, token, init);

export const interviewService = {
  getMine: (token: string, signal?: AbortSignal) =>
    request<InterviewSummary[]>('/me', token, { signal }),

  getById: (id: string, token: string, signal?: AbortSignal) =>
    request<InterviewDetail>(`/${id}`, token, { signal }),

  start: (id: string, token: string) =>
    request<InterviewDetail>(`/${id}/start`, token, { method: 'POST' }),

  /** Descarga el PDF como Blob (no JSON): un <a href> común no sirve porque hay que mandar el JWT. */
  async downloadCertificate(id: string, token: string): Promise<{ blob: Blob; fileName: string }> {
    const res = await fetch(`${INTERVIEWS_URL}/${id}/certificate`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!res.ok) {
      const problem = await res.json().catch(() => ({}));
      throw new ApiError(res.status, problem.detail || problem.title || 'No se pudo generar el certificado.');
    }

    // El nombre sugerido viene en el header Content-Disposition: attachment; filename=...
    const disposition = res.headers.get('Content-Disposition') ?? '';
    const fileName = /filename="?([^";]+)"?/.exec(disposition)?.[1] ?? `certificado-mockly-${id.slice(0, 8)}.pdf`;

    return { blob: await res.blob(), fileName };
  },

  submit: (id: string, code: string, token: string) =>
    request<EvaluationResult>(`/${id}/submit`, token, {
      method: 'POST',
      body: JSON.stringify({ code }),
    }),
};
