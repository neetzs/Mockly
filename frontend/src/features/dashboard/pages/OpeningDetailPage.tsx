import React, { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, BarChart3, ChevronRight, Loader2, Lock, UserPlus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { useAuth } from '@/features/auth/context';
import { ApiError } from '@/features/interviews/services/interview.service';
import { difficultyLabel, languageLabel } from '@/features/interviews/languages';
import { STATUS_LABEL } from '@/features/interviews/status';
import { recruiterService } from '../services/recruiter.service';
import type { InvitedCandidate, JobOpeningDetail } from '../types/dashboard.types';

/** Gestión de una búsqueda: sus retos, invitar candidatos y seguir el avance de cada uno. */
export const OpeningDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { token, logout } = useAuth();

  const [opening, setOpening] = useState<JobOpeningDetail | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);

  useEffect(() => {
    if (!token || !id) return;
    const controller = new AbortController();
    recruiterService
      .getOpening(id, token, controller.signal)
      .then(setOpening)
      .catch((err: unknown) => {
        if (controller.signal.aborted) return;
        if (err instanceof ApiError && err.status === 401) logout();
        setLoadError(err instanceof Error ? err.message : 'No se pudo cargar la búsqueda.');
      });
    return () => controller.abort();
  }, [id, token, logout]);

  if (loadError) return <p className="text-sm text-danger">{loadError}</p>;
  if (!opening) return <Loader2 className="h-6 w-6 animate-spin text-brand mx-auto" />;

  // El candidato invitado se suma a la lista sin volver a pedir la búsqueda entera.
  const handleInvited = (candidate: InvitedCandidate) =>
    setOpening((prev) =>
      prev && {
        ...prev,
        candidates: [...prev.candidates, candidate].sort((a, b) => a.fullName.localeCompare(b.fullName)),
      },
    );

  return (
    <div className="w-full max-w-3xl mx-auto space-y-8 text-left">
      <Link to="/dashboard" className="inline-flex items-center gap-1.5 text-xs text-muted-foreground hover:text-foreground">
        <ArrowLeft className="h-3.5 w-3.5" />
        Dashboard
      </Link>

      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="min-w-0 space-y-1">
          <h2 className="text-2xl font-bold text-foreground">{opening.title}</h2>
          {opening.description && <p className="text-sm text-muted-foreground">{opening.description}</p>}
        </div>
        <div className="flex shrink-0 flex-wrap items-center gap-2">
          <Link
            to={`/dashboard?opening=${opening.id}`}
            className="inline-flex shrink-0 items-center gap-1.5 rounded-lg border border-border bg-surface/50 px-3 py-1.5 text-sm text-soft transition-colors hover:border-indigo-500/40 hover:text-foreground"
          >
            <BarChart3 className="h-4 w-4" />
            Ver ranking
          </Link>
          {opening.isActive ? (
            <CloseOpeningButton openingId={opening.id} onClosed={setOpening} />
          ) : (
            <span className="inline-flex shrink-0 items-center gap-1.5 rounded-lg border border-input bg-muted/60 px-3 py-1.5 text-sm text-soft">
              <Lock className="h-4 w-4" />
              Búsqueda cerrada
            </span>
          )}
        </div>
      </div>

      <section className="space-y-3">
        <h3 className="text-sm font-medium text-muted-foreground">Retos de la búsqueda</h3>
        <ol className="flex flex-wrap gap-2">
          {opening.challenges.map((challenge) => (
            <li key={challenge.id} className="rounded-lg border border-border bg-surface/40 px-3 py-1.5 text-sm">
              <span className="text-foreground">{challenge.title}</span>{' '}
              <span className="text-xs text-subtle font-mono">
                {languageLabel(challenge.language)} · {difficultyLabel(challenge.difficulty)}
              </span>
            </li>
          ))}
        </ol>
      </section>

      <InviteForm openingId={opening.id} disabled={!opening.isActive} onInvited={handleInvited} />

      <section className="space-y-3">
        <h3 className="text-sm font-medium text-muted-foreground">
          Candidatos invitados <span className="font-mono">({opening.candidates.length})</span>
        </h3>
        {opening.candidates.length === 0 ? (
          <p className="rounded-xl border border-dashed border-border px-6 py-8 text-center text-sm text-muted-foreground">
            Todavía no invitaste a nadie. Cuando invites a un candidato, le aparecen los retos en su cuenta.
          </p>
        ) : (
          <ul className="space-y-3">
            {opening.candidates.map((candidate) => (
              <CandidateCard key={candidate.candidateId} candidate={candidate} />
            ))}
          </ul>
        )}
      </section>
    </div>
  );
};

const CloseOpeningButton: React.FC<{ openingId: string; onClosed: (o: JobOpeningDetail) => void }> = ({
  openingId,
  onClosed,
}) => {
  const { token, logout } = useAuth();
  const [open, setOpen] = useState(false);
  const [isClosing, setIsClosing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleConfirm = async () => {
    if (!token) return;
    setIsClosing(true);
    setError(null);
    try {
      onClosed(await recruiterService.closeOpening(openingId, token));
      setOpen(false);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) logout();
      setError(err instanceof Error ? err.message : 'No se pudo cerrar la búsqueda.');
    } finally {
      setIsClosing(false);
    }
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        if (isClosing) return;
        setError(null); // cada apertura arranca limpia, sin el error del intento anterior
        setOpen(next);
      }}
    >
      <DialogTrigger asChild>
        <Button variant="outline" className="shrink-0">
          <Lock className="h-4 w-4" />
          Cerrar búsqueda
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>¿Cerrar la búsqueda?</DialogTitle>
          <DialogDescription>
            No vas a poder invitar más candidatos. Los que ya invitaste pueden terminar sus retos y vas a seguir viendo
            sus resultados. Esta acción no se puede deshacer.
          </DialogDescription>
        </DialogHeader>
        {error && <p className="text-sm text-danger">{error}</p>}
        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline" disabled={isClosing}>
              Cancelar
            </Button>
          </DialogClose>
          <Button variant="destructive" onClick={handleConfirm} disabled={isClosing}>
            {isClosing && <Loader2 className="h-4 w-4 animate-spin" />}
            Cerrar búsqueda
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

const InviteForm: React.FC<{ openingId: string; disabled: boolean; onInvited: (c: InvitedCandidate) => void }> = ({
  openingId,
  disabled,
  onInvited,
}) => {
  const { token, logout } = useAuth();
  const [email, setEmail] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [message, setMessage] = useState<{ ok: boolean; text: string } | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!token) return;
    setIsSending(true);
    setMessage(null);
    try {
      const invited = await recruiterService.invite(openingId, email.trim(), token);
      onInvited(invited);
      setEmail('');
      setMessage({ ok: true, text: `Invitaste a ${invited.fullName}. Ya ve los retos en su cuenta.` });
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) logout();
      setMessage({ ok: false, text: err instanceof Error ? err.message : 'No se pudo enviar la invitación.' });
    } finally {
      setIsSending(false);
    }
  };

  return (
    <section className="space-y-2">
      <h3 className="text-sm font-medium text-muted-foreground">Invitar candidato</h3>
      <form onSubmit={handleSubmit} className="flex flex-col gap-2 sm:flex-row">
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          disabled={disabled}
          placeholder="email@candidato.com"
          aria-label="Email del candidato"
          className="min-w-0 flex-1 rounded-lg border border-input bg-surface px-3 py-2 text-sm text-foreground placeholder:text-faint focus:outline-none focus:ring-2 focus:ring-ring"
        />
        <Button type="submit" disabled={disabled || isSending}>
          {isSending ? <Loader2 className="h-4 w-4 animate-spin" /> : <UserPlus className="h-4 w-4" />}
          Invitar
        </Button>
      </form>
      <p className="text-xs text-muted-foreground">El candidato tiene que tener una cuenta en Mockly con ese email.</p>
      {message && <p className={`text-sm ${message.ok ? 'text-success' : 'text-danger'}`}>{message.text}</p>}
    </section>
  );
};

const CandidateCard: React.FC<{ candidate: InvitedCandidate }> = ({ candidate }) => {
  const done = candidate.sessions.filter((s) => s.status === 'Completed').length;
  return (
    <li className="rounded-xl border border-border bg-surface/40 px-5 py-4 space-y-3">
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <div className="min-w-0">
          <p className="font-semibold text-foreground">{candidate.fullName}</p>
          <p className="text-xs text-muted-foreground truncate">{candidate.email}</p>
        </div>
        <p className="text-xs text-muted-foreground">
          {done} de {candidate.sessions.length} completados
        </p>
      </div>
      <ul className="divide-y divide-border">
        {candidate.sessions.map((session) => {
          const status = STATUS_LABEL[session.status];
          // Completadas y fallidas tienen código enviado para revisar.
          const reviewable = session.status === 'Completed' || session.status === 'Failed';
          const row = (
            <>
              <span className="min-w-0 truncate text-sm text-soft">{session.challengeTitle}</span>
              <span className="flex shrink-0 items-center gap-3">
                {session.overallScore !== null && (
                  <span className="font-mono text-sm text-foreground">{session.overallScore}/100</span>
                )}
                <span className={`whitespace-nowrap rounded-full border px-2 py-0.5 text-[11px] font-medium ${status.className}`}>
                  {status.text}
                </span>
                {reviewable && <ChevronRight className="h-4 w-4 text-subtle" />}
              </span>
            </>
          );
          return (
            <li key={session.sessionId}>
              {reviewable ? (
                <Link
                  to={`/dashboard/sessions/${session.sessionId}`}
                  className="flex items-center justify-between gap-3 py-2 hover:text-foreground"
                  aria-label={`Ver el código de ${candidate.fullName} en ${session.challengeTitle}`}
                >
                  {row}
                </Link>
              ) : (
                <div className="flex items-center justify-between gap-3 py-2">{row}</div>
              )}
            </li>
          );
        })}
      </ul>
    </li>
  );
};
