import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ChevronRight, Loader2 } from 'lucide-react';
import { useAuth } from '../../auth/context';
import { ApiError, interviewService } from '../services/interview.service';
import { languageLabel } from '../languages';
import type { InterviewStatus, InterviewSummary } from '../types/interview.types';

const STATUS_LABEL: Record<InterviewStatus, { text: string; className: string }> = {
  NotStarted: { text: 'Pendiente', className: 'text-soft border-input bg-muted/60' },
  InProgress: { text: 'En curso', className: 'text-brand border-indigo-500/30 bg-indigo-500/10' },
  Completed: { text: 'Completada', className: 'text-success border-emerald-500/30 bg-emerald-500/10' },
  Failed: { text: 'Fallida', className: 'text-danger border-red-500/30 bg-red-500/10' },
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
    return <p className="text-sm text-danger">{error}</p>;
  }

  if (!interviews) {
    return <Loader2 className="h-6 w-6 animate-spin text-brand mx-auto" />;
  }

  return (
    <div className="w-full max-w-3xl mx-auto space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-foreground">Mis entrevistas</h2>
        <p className="text-sm text-muted-foreground">Elegí un reto para entrar a la sala de código.</p>
      </div>

      {interviews.length === 0 ? (
        <p className="text-sm text-muted-foreground">Todavía no tenés entrevistas asignadas.</p>
      ) : (
        <ul className="space-y-3">
          {interviews.map((interview) => {
            const status = STATUS_LABEL[interview.status];
            return (
              <li key={interview.id}>
                <Link
                  to={`/interviews/${interview.id}`}
                  className="flex items-center justify-between gap-4 rounded-xl border border-border bg-surface/40 hover:border-indigo-500/40 hover:bg-surface/80 transition-colors px-5 py-4"
                >
                  <div className="space-y-1 text-left">
                    <p className="font-semibold text-foreground">{interview.challengeTitle}</p>
                    <p className="text-xs text-subtle font-mono">
                      {languageLabel(interview.language)} · {interview.difficulty}
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    {interview.overallScore !== null && (
                      <span className="font-mono text-sm text-foreground">{interview.overallScore}/100</span>
                    )}
                    <span className={`text-[11px] font-medium px-2 py-0.5 rounded-full border ${status.className}`}>
                      {status.text}
                    </span>
                    <ChevronRight className="h-4 w-4 text-subtle" />
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
