import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ChevronRight, Loader2 } from 'lucide-react';
import { useAuth } from '../../auth/context';
import { ApiError, interviewService } from '../services/interview.service';
import type { InterviewStatus, InterviewSummary } from '../types/interview.types';

const STATUS_LABEL: Record<InterviewStatus, { text: string; className: string }> = {
  NotStarted: { text: 'Pendiente', className: 'text-slate-300 border-slate-700 bg-slate-800/60' },
  InProgress: { text: 'En curso', className: 'text-indigo-300 border-indigo-500/30 bg-indigo-500/10' },
  Completed: { text: 'Completada', className: 'text-emerald-300 border-emerald-500/30 bg-emerald-500/10' },
  Failed: { text: 'Fallida', className: 'text-red-300 border-red-500/30 bg-red-500/10' },
};

export const MyInterviewsPage: React.FC = () => {
  const { token, logout } = useAuth();
  const [interviews, setInterviews] = useState<InterviewSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) return;

    // AbortController: si el componente se desmonta antes de que llegue la respuesta,
    // cancelamos el fetch para no actualizar el estado de un componente que ya no existe.
    const controller = new AbortController();

    interviewService
      .getMine(token, controller.signal)
      .then(setInterviews)
      .catch((err: unknown) => {
        if (controller.signal.aborted) return;
        if (err instanceof ApiError && err.status === 401) logout();
        setError(err instanceof Error ? err.message : 'No se pudieron cargar las entrevistas.');
      });

    // La función que devuelve useEffect es el "cleanup": corre al desmontar.
    return () => controller.abort();
  }, [token, logout]);

  if (error) {
    return <p className="text-sm text-red-400">{error}</p>;
  }

  if (!interviews) {
    return <Loader2 className="h-6 w-6 animate-spin text-indigo-400 mx-auto" />;
  }

  return (
    <div className="w-full max-w-3xl mx-auto space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-white">Mis entrevistas</h2>
        <p className="text-sm text-slate-400">Elegí un reto para entrar a la sala de código.</p>
      </div>

      {interviews.length === 0 ? (
        <p className="text-sm text-slate-400">Todavía no tenés entrevistas asignadas.</p>
      ) : (
        <ul className="space-y-3">
          {interviews.map((interview) => {
            const status = STATUS_LABEL[interview.status];
            return (
              <li key={interview.id}>
                <Link
                  to={`/interviews/${interview.id}`}
                  className="flex items-center justify-between gap-4 rounded-xl border border-slate-800 bg-slate-900/40 hover:border-indigo-500/40 hover:bg-slate-900/80 transition-colors px-5 py-4"
                >
                  <div className="space-y-1 text-left">
                    <p className="font-semibold text-slate-100">{interview.challengeTitle}</p>
                    <p className="text-xs text-slate-500 font-mono">{interview.difficulty}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    {interview.overallScore !== null && (
                      <span className="font-mono text-sm text-slate-200">{interview.overallScore}/100</span>
                    )}
                    <span className={`text-[11px] font-medium px-2 py-0.5 rounded-full border ${status.className}`}>
                      {status.text}
                    </span>
                    <ChevronRight className="h-4 w-4 text-slate-500" />
                  </div>
                </Link>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
};
