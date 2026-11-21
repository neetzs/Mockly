import React, { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import Editor from '@monaco-editor/react';
import { ArrowLeft, Loader2, Play, RotateCcw, Send } from 'lucide-react';
import { useAuth } from '../../auth/context';
import { ApiError, interviewService } from '../services/interview.service';
import { EvaluationPanel } from '../components/EvaluationPanel';
import { CertificateDownloadButton } from '../components/CertificateDownloadButton';
import { difficultyLabel, languageLabel, monacoLanguageId } from '../languages';
import { practiceService } from '@/features/practice/services/practice.service';
import { useTheme } from '@/features/theme';
import type { InterviewDetail } from '../types/interview.types';

/**
 * Sala de entrevista. Flujo contra la API:
 *   1. GET  /interviews/{id}         → carga el reto (y la evaluación si ya terminó)
 *   2. POST /interviews/{id}/start   → NotStarted → InProgress (botón "Comenzar")
 *   3. POST /interviews/{id}/submit  → envía el código y recibe la evaluación de la IA
 */
export const InterviewRoomPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { token, logout } = useAuth();
  const { resolvedTheme } = useTheme();
  const navigate = useNavigate();

  const [interview, setInterview] = useState<InterviewDetail | null>(null);
  const [code, setCode] = useState('');
  const [loadError, setLoadError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [isBusy, setIsBusy] = useState(false);

  // Actualiza la sala con la respuesta del backend. Si ya hay código enviado lo mostramos;
  // si no, arrancamos con la plantilla del reto.
  const applyDetail = (detail: InterviewDetail) => {
    setInterview(detail);
    setCode(detail.submittedCode ?? detail.initialCodeTemplate);
  };

  // Paso 1 — Cargar la entrevista al entrar (o si cambia el id de la URL).
  useEffect(() => {
    if (!token || !id) return;
    const controller = new AbortController();

    interviewService
      .getById(id, token, controller.signal)
      .then(applyDetail)
      .catch((err: unknown) => {
        if (controller.signal.aborted) return;
        if (err instanceof ApiError && err.status === 401) logout();
        setLoadError(err instanceof Error ? err.message : 'No se pudo cargar la entrevista.');
      });

    return () => controller.abort();
  }, [id, token, logout]);

  // Envuelve las acciones (start / submit): maneja el "cargando" y los errores en un solo lugar.
  const runAction = async (action: () => Promise<void>) => {
    setIsBusy(true);
    setActionError(null);
    try {
      await action();
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) logout();
      setActionError(err instanceof Error ? err.message : 'Ocurrió un error inesperado.');
    } finally {
      setIsBusy(false);
    }
  };

  // Paso 2 — Comenzar la entrevista.
  const handleStart = () =>
    runAction(async () => {
      if (!token || !id) return;
      applyDetail(await interviewService.start(id, token));
    });

  // Paso 3 — Enviar el código. Con la respuesta marcamos la sesión como completada en la UI
  // sin volver a pedirla al backend.
  const handleSubmit = () =>
    runAction(async () => {
      if (!token || !id || !interview) return;
      const evaluation = await interviewService.submit(id, code, token);
      setInterview({ ...interview, status: 'Completed', submittedCode: code, evaluation });
    });

  // Práctica: un intento nuevo del mismo reto (el backend retoma uno en curso si existe).
  const handlePracticeAgain = () =>
    runAction(async () => {
      if (!token || !interview) return;
      const attempt = await practiceService.startAttempt(interview.challengeId, token);
      navigate(`/interviews/${attempt.id}`);
    });

  if (loadError) {
    return (
      <div className="space-y-4 text-center">
        <p className="text-sm text-danger">{loadError}</p>
        <BackLink isPractice={false} />
      </div>
    );
  }

  if (!interview) {
    return <Loader2 className="h-6 w-6 animate-spin text-brand mx-auto" />;
  }

  const isPractice = interview.jobOpeningTitle === null;
  const isInProgress = interview.status === 'InProgress';
  const isFinished = interview.status === 'Completed' || interview.status === 'Failed';

  return (
    <div className="w-full grid gap-6 lg:grid-cols-[minmax(0,2fr)_minmax(0,3fr)] text-left">
      {/* Columna izquierda: enunciado + acciones + resultado */}
      <aside className="space-y-5">
        <BackLink isPractice={isPractice} />

        <div className="space-y-2">
          <span className="text-[11px] font-mono uppercase tracking-wider text-brand">
            {languageLabel(interview.language)} · {difficultyLabel(interview.difficulty)}
          </span>
          <p className="text-xs text-muted-foreground">
            {isPractice ? 'Práctica: el resultado no lo ve ningún reclutador.' : `Búsqueda: ${interview.jobOpeningTitle}`}
          </p>
          <h2 className="text-2xl font-bold text-foreground">{interview.challengeTitle}</h2>
          <p className="text-sm text-soft leading-relaxed whitespace-pre-line">
            {unwrapLines(interview.challengeDescription)}
          </p>
        </div>

        {interview.status === 'NotStarted' && (
          <button
            type="button"
            onClick={handleStart}
            disabled={isBusy}
            className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 hover:bg-indigo-500 disabled:opacity-60 px-4 py-2 text-sm font-semibold text-white transition-colors"
          >
            {isBusy ? <Loader2 className="h-4 w-4 animate-spin" /> : <Play className="h-4 w-4" />}
            Comenzar entrevista
          </button>
        )}

        {isInProgress && (
          <button
            type="button"
            onClick={handleSubmit}
            disabled={isBusy}
            className="inline-flex items-center gap-2 rounded-lg bg-emerald-600 hover:bg-emerald-500 disabled:opacity-60 px-4 py-2 text-sm font-semibold text-white transition-colors"
          >
            {isBusy ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />}
            {isBusy ? 'Evaluando con IA…' : 'Enviar solución'}
          </button>
        )}

        {actionError && <p className="text-sm text-danger">{actionError}</p>}

        {interview.evaluation && (
          <>
            <EvaluationPanel evaluation={interview.evaluation} />
            <CertificateDownloadButton interviewId={interview.id} />
            {isPractice && (
              <button
                type="button"
                onClick={handlePracticeAgain}
                disabled={isBusy}
                className="inline-flex items-center gap-2 rounded-lg border border-border bg-surface/50 px-4 py-2 text-sm text-soft transition-colors hover:border-indigo-500/40 hover:text-foreground disabled:opacity-60"
              >
                {isBusy ? <Loader2 className="h-4 w-4 animate-spin" /> : <RotateCcw className="h-4 w-4" />}
                Practicar de nuevo
              </button>
            )}
          </>
        )}
      </aside>

      {/* Columna derecha: editor Monaco (solo lectura si no está en curso) */}
      <div className="relative rounded-xl border border-border overflow-hidden min-h-[480px]">
        <Editor
          height="100%"
          language={monacoLanguageId(interview.language)}
          theme={resolvedTheme === 'dark' ? 'vs-dark' : 'light'}
          value={code}
          onChange={(value) => setCode(value ?? '')}
          loading={<Loader2 className="h-6 w-6 animate-spin text-brand" />}
          options={{
            readOnly: !isInProgress || isBusy,
            minimap: { enabled: false },
            fontSize: 14,
            scrollBeyondLastLine: false,
            automaticLayout: true,
            tabSize: 4,
          }}
        />

        {interview.status === 'NotStarted' && (
          <Overlay text="Presioná “Comenzar entrevista” para empezar a escribir." />
        )}
        {isFinished && !interview.evaluation && <Overlay text="Esta entrevista ya finalizó." />}
      </div>
    </div>
  );
};

// Las descripciones vienen con saltos de línea "de código" (cortadas a ~100 columnas en el seeder).
// Un salto simple se une con un espacio para que el texto fluya; uno doble sigue separando párrafos.
const unwrapLines = (text: string) => text.replace(/([^\n])\n(?!\n)/g, '$1 ');

const BackLink: React.FC<{ isPractice: boolean }> = ({ isPractice }) => (
  <Link
    to={isPractice ? '/practice' : '/interviews'}
    className="inline-flex items-center gap-1.5 text-xs text-muted-foreground hover:text-foreground"
  >
    <ArrowLeft className="h-3.5 w-3.5" />
    {isPractice ? 'Biblioteca de práctica' : 'Mis entrevistas'}
  </Link>
);

const Overlay: React.FC<{ text: string }> = ({ text }) => (
  <div className="absolute inset-0 flex items-center justify-center bg-background/60 backdrop-blur-[1px]">
    <p className="text-sm text-soft bg-surface border border-border rounded-lg px-4 py-2">{text}</p>
  </div>
);
