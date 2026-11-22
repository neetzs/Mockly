import React, { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import Editor from '@monaco-editor/react';
import { ArrowLeft, Loader2 } from 'lucide-react';
import { useAuth } from '@/features/auth/context';
import { useTheme } from '@/features/theme';
import { ApiError } from '@/features/interviews/services/interview.service';
import { EvaluationPanel } from '@/features/interviews/components/EvaluationPanel';
import { difficultyLabel, languageLabel, monacoLanguageId } from '@/features/interviews/languages';
import { recruiterService } from '../services/recruiter.service';
import type { SessionReview } from '../types/dashboard.types';

/**
 * Revisión de una entrevista: el código que envió el candidato (solo lectura) junto a la devolución de la IA.
 * El puntaje ordena, pero la decisión la toma el reclutador mirando el código.
 */
export const SessionReviewPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { token, logout } = useAuth();
  const { resolvedTheme } = useTheme();

  const [review, setReview] = useState<SessionReview | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token || !id) return;
    const controller = new AbortController();
    recruiterService
      .getSessionReview(id, token, controller.signal)
      .then(setReview)
      .catch((err: unknown) => {
        if (controller.signal.aborted) return;
        if (err instanceof ApiError && err.status === 401) logout();
        setError(err instanceof Error ? err.message : 'No se pudo cargar la entrevista.');
      });
    return () => controller.abort();
  }, [id, token, logout]);

  if (error) return <p className="text-sm text-danger">{error}</p>;
  if (!review) return <Loader2 className="h-6 w-6 animate-spin text-brand mx-auto" />;

  const completedAt = review.completedAtUtc
    ? new Date(review.completedAtUtc).toLocaleString('es-AR', { dateStyle: 'medium', timeStyle: 'short' })
    : null;

  return (
    <div className="w-full grid gap-6 lg:grid-cols-[minmax(0,2fr)_minmax(0,3fr)] text-left">
      <aside className="space-y-5">
        <Link
          to={`/dashboard/openings/${review.jobOpeningId}`}
          className="inline-flex items-center gap-1.5 text-xs text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-3.5 w-3.5" />
          {review.jobOpeningTitle}
        </Link>

        <div className="space-y-1">
          <h2 className="text-2xl font-bold text-foreground">{review.candidateName}</h2>
          <p className="text-sm text-muted-foreground">{review.candidateEmail}</p>
        </div>

        <div className="space-y-2">
          <span className="text-[11px] font-mono uppercase tracking-wider text-brand">
            {languageLabel(review.language)} · {difficultyLabel(review.difficulty)}
          </span>
          <h3 className="text-lg font-semibold text-foreground">{review.challengeTitle}</h3>
          <p className="text-sm text-soft leading-relaxed whitespace-pre-line">{unwrapLines(review.challengeDescription)}</p>
          {completedAt && <p className="text-xs text-muted-foreground">Enviado el {completedAt}</p>}
        </div>

        {review.evaluation ? (
          <EvaluationPanel evaluation={review.evaluation} />
        ) : (
          <p className="text-sm text-muted-foreground">Esta entrevista todavía no fue evaluada.</p>
        )}
      </aside>

      <div className="relative rounded-xl border border-border overflow-hidden min-h-[480px]">
        <Editor
          height="100%"
          language={monacoLanguageId(review.language)}
          theme={resolvedTheme === 'dark' ? 'vs-dark' : 'light'}
          value={review.submittedCode ?? '// El candidato todavía no envió código.'}
          loading={<Loader2 className="h-6 w-6 animate-spin text-brand" />}
          options={{
            readOnly: true,
            minimap: { enabled: false },
            fontSize: 14,
            scrollBeyondLastLine: false,
            automaticLayout: true,
          }}
        />
      </div>
    </div>
  );
};

// Igual que en la sala: une los saltos de línea simples del seeder para que el texto fluya.
const unwrapLines = (text: string) => text.replace(/([^\n])\n(?!\n)/g, '$1 ');
