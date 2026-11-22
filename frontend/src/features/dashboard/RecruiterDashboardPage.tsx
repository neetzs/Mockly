import React, { useEffect, useRef, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Activity, BellRing, Code2, Loader2, Plus, Settings2 } from 'lucide-react';
import { useAuth } from '../auth/context';
import { useSignalR, type ConnectionStatus } from '@/hooks/useSignalR';
import { INTERVIEW_HUB_URL, recruiterService } from './services/recruiter.service';
import { LiveLeaderboard } from './components/LiveLeaderboard';
import { CandidateRadarChart } from './components/CandidateRadarChart';
import type { CandidateRanking, InterviewCompletedEvent, JobOpeningSummary } from './types/dashboard.types';

const HIGHLIGHT_MS = 4000;

/** Mantiene el ranking ordenado: mejor puntaje primero; ante empate, el que terminó antes. */
const sortRanking = (rows: CandidateRanking[]): CandidateRanking[] =>
  [...rows].sort(
    (a, b) => b.overallScore - a.overallScore || a.completedAtUtc.localeCompare(b.completedAtUtc),
  );

export const RecruiterDashboardPage: React.FC = () => {
  const { token } = useAuth();
  // ?opening=<id> permite llegar al ranking de una búsqueda puntual (desde su página de gestión).
  const [searchParams] = useSearchParams();

  const [openings, setOpenings] = useState<JobOpeningSummary[] | null>(null);
  const [openingId, setOpeningId] = useState<string | null>(null);
  const [ranking, setRanking] = useState<CandidateRanking[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [highlightedId, setHighlightedId] = useState<string | null>(null);
  const [toast, setToast] = useState<CandidateRanking | null>(null);
  const [error, setError] = useState<string | null>(null);
  const highlightTimer = useRef<number | undefined>(undefined);

  // 1) Búsquedas del reclutador (al entrar). Seleccionamos la primera.
  useEffect(() => {
    if (!token) return;
    const controller = new AbortController();
    recruiterService
      .getOpenings(token, controller.signal)
      .then((data) => {
        setOpenings(data);
        const requested = searchParams.get('opening');
        const initial = data.find((o) => o.id === requested) ?? data[0];
        setOpeningId((current) => current ?? initial?.id ?? null);
      })
      .catch((err: unknown) => {
        if (!controller.signal.aborted) setError(err instanceof Error ? err.message : 'Error al cargar.');
      });
    return () => controller.abort();
  }, [token, searchParams]);

  // 2) Ranking inicial de la búsqueda seleccionada (lo que pasó ANTES de abrir el dashboard).
  useEffect(() => {
    if (!token || !openingId) return;
    const controller = new AbortController();
    recruiterService
      .getRanking(openingId, token, controller.signal)
      .then((data) => {
        setRanking(data);
        setSelectedId(data[0]?.sessionId ?? null);
      })
      .catch((err: unknown) => {
        if (!controller.signal.aborted) setError(err instanceof Error ? err.message : 'Error al cargar.');
      });
    return () => controller.abort();
  }, [token, openingId]);

  // 3) Tiempo real: lo que pasa DESPUÉS llega por SignalR, sin recargar.
  const status = useSignalR(INTERVIEW_HUB_URL, token, {
    InterviewCompleted: (event: InterviewCompletedEvent) => {
      setOpenings((prev) =>
        prev?.map((o) =>
          o.id === event.jobOpeningId ? { ...o, completedInterviews: o.completedInterviews + 1 } : o,
        ) ?? prev,
      );

      if (event.jobOpeningId !== openingId) return;

      // Usamos la forma funcional (prev => ...) porque el evento puede llegar en cualquier momento
      // y queremos operar sobre el estado más reciente, no sobre una copia vieja.
      setRanking((prev) =>
        sortRanking([...prev.filter((r) => r.sessionId !== event.entry.sessionId), event.entry]),
      );
      setSelectedId(event.entry.sessionId);
      setHighlightedId(event.entry.sessionId);
      setToast(event.entry);

      window.clearTimeout(highlightTimer.current);
      highlightTimer.current = window.setTimeout(() => {
        setHighlightedId(null);
        setToast(null);
      }, HIGHLIGHT_MS);
    },
  });

  useEffect(() => () => window.clearTimeout(highlightTimer.current), []);

  if (error) return <p className="text-sm text-danger">{error}</p>;
  if (!openings) return <Loader2 className="h-6 w-6 animate-spin text-brand mx-auto" />;

  // Reclutador nuevo: en vez de un dashboard vacío, lo invitamos a crear su primera búsqueda.
  if (openings.length === 0) {
    return (
      <div className="w-full max-w-xl mx-auto rounded-xl border border-dashed border-border px-6 py-12 text-center space-y-3">
        <h2 className="text-xl font-bold text-foreground">Creá tu primera búsqueda</h2>
        <p className="text-sm text-muted-foreground">
          Elegí los retos de la biblioteca, invitá candidatos y seguí sus resultados en vivo desde acá.
        </p>
        <Link
          to="/dashboard/openings/new"
          className="inline-flex items-center gap-1.5 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-indigo-500"
        >
          <Plus className="h-4 w-4" />
          Nueva búsqueda
        </Link>
      </div>
    );
  }

  const selected = ranking.find((r) => r.sessionId === selectedId) ?? ranking[0];
  const scores = ranking.map((r) => r.overallScore);
  const avg = scores.length ? Math.round(scores.reduce((a, b) => a + b, 0) / scores.length) : 0;

  return (
    <div className="w-full space-y-6 text-left">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div className="space-y-1">
          <h2 className="text-2xl font-bold text-foreground">Dashboard de reclutamiento</h2>
          {openings.length > 1 ? (
            <select
              value={openingId ?? ''}
              onChange={(e) => setOpeningId(e.target.value)}
              className="bg-surface border border-border rounded-lg px-3 py-1.5 text-sm text-foreground"
            >
              {openings.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.title}
                </option>
              ))}
            </select>
          ) : (
            <p className="text-sm text-muted-foreground">{openings[0].title}</p>
          )}
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <LiveIndicator status={status} />
          {openingId && (
            <Link
              to={`/dashboard/openings/${openingId}`}
              className="inline-flex items-center gap-1.5 rounded-lg border border-border bg-surface/50 px-3 py-1.5 text-sm text-soft transition-colors hover:border-indigo-500/40 hover:text-foreground"
            >
              <Settings2 className="h-4 w-4" />
              Gestionar
            </Link>
          )}
          <Link
            to="/dashboard/openings/new"
            className="inline-flex items-center gap-1.5 rounded-lg bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white transition-colors hover:bg-indigo-500"
          >
            <Plus className="h-4 w-4" />
            Nueva búsqueda
          </Link>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <Stat label="Completadas" value={ranking.length} />
        <Stat label="Promedio" value={avg} />
        <Stat label="Mejor puntaje" value={scores.length ? Math.max(...scores) : 0} />
      </div>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,3fr)_minmax(0,2fr)]">
        <LiveLeaderboard
          ranking={ranking}
          selectedId={selected?.sessionId ?? null}
          highlightedId={highlightedId}
          onSelect={setSelectedId}
        />
        {selected && (
          <div className="space-y-3">
            <CandidateRadarChart candidate={selected} ranking={ranking} />
            {/* El puntaje ordena; la decisión se toma leyendo el código */}
            <Link
              to={`/dashboard/sessions/${selected.sessionId}`}
              className="flex items-center justify-center gap-2 rounded-lg border border-border bg-surface/50 px-4 py-2 text-sm text-soft transition-colors hover:border-indigo-500/40 hover:text-foreground"
            >
              <Code2 className="h-4 w-4" />
              Ver código y devolución de {selected.candidateName}
            </Link>
          </div>
        )}
      </div>

      {toast && (
        <div className="fixed bottom-6 right-6 z-50 flex items-center gap-3 rounded-xl border border-emerald-500/30 bg-surface px-4 py-3 shadow-lg shadow-emerald-900/20">
          <BellRing className="h-4 w-4 text-success" />
          <p className="text-sm text-foreground">
            <span className="font-semibold">{toast.candidateName}</span> terminó{' '}
            <span className="text-muted-foreground">{toast.challengeTitle}</span> con{' '}
            <span className="font-mono font-semibold text-success">{toast.overallScore}</span>
          </p>
        </div>
      )}
    </div>
  );
};

const STATUS_UI: Record<ConnectionStatus, { text: string; dot: string }> = {
  connected: { text: 'En vivo', dot: 'bg-emerald-400 animate-pulse' },
  connecting: { text: 'Conectando…', dot: 'bg-amber-400' },
  reconnecting: { text: 'Reconectando…', dot: 'bg-amber-400 animate-pulse' },
  disconnected: { text: 'Sin conexión en vivo', dot: 'bg-red-400' },
};

const LiveIndicator: React.FC<{ status: ConnectionStatus }> = ({ status }) => (
  <div className="flex items-center gap-2 rounded-full border border-border bg-surface px-3 py-1.5 text-xs text-soft">
    <Activity className="h-3.5 w-3.5 text-subtle" />
    <span className={`h-2 w-2 rounded-full ${STATUS_UI[status].dot}`} />
    {STATUS_UI[status].text}
  </div>
);

const Stat: React.FC<{ label: string; value: number }> = ({ label, value }) => (
  <div className="rounded-xl border border-border bg-surface/40 px-4 py-3">
    <p className="text-[11px] uppercase tracking-wider text-subtle">{label}</p>
    <p className="text-2xl font-bold font-mono text-foreground">{value}</p>
  </div>
);
