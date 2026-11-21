import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Loader2, Play } from 'lucide-react';
import { useAuth } from '@/features/auth/context';
import { ApiError } from '@/features/interviews/services/interview.service';
import { difficultyLabel, languageLabel, type ProgrammingLanguage } from '@/features/interviews/languages';
import { practiceService } from '../services/practice.service';
import type { PracticeChallenge } from '../types/practice.types';

/**
 * Biblioteca de práctica: retos que cualquier candidato puede resolver las veces que quiera.
 * "Practicar" crea un intento en el backend y lleva a la misma sala que usan las entrevistas.
 */
export const PracticePage: React.FC = () => {
  const { token, logout } = useAuth();
  const navigate = useNavigate();

  const [challenges, setChallenges] = useState<PracticeChallenge[] | null>(null);
  const [language, setLanguage] = useState<ProgrammingLanguage | 'all'>('all');
  const [startingId, setStartingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) return;
    const controller = new AbortController();

    practiceService
      .getChallenges(token, controller.signal)
      .then(setChallenges)
      .catch((err: unknown) => {
        if (controller.signal.aborted) return;
        if (err instanceof ApiError && err.status === 401) logout();
        setError(err instanceof Error ? err.message : 'No se pudo cargar la biblioteca.');
      });

    return () => controller.abort();
  }, [token, logout]);

  // useMemo: la lista de lenguajes solo se recalcula cuando cambian los retos, no en cada render.
  const languages = useMemo(
    () => [...new Set((challenges ?? []).map((c) => c.language))],
    [challenges],
  );

  const handlePractice = async (challengeId: string) => {
    if (!token) return;
    setStartingId(challengeId);
    setError(null);
    try {
      const attempt = await practiceService.startAttempt(challengeId, token);
      navigate(`/interviews/${attempt.id}`);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) logout();
      setError(err instanceof Error ? err.message : 'No se pudo iniciar la práctica.');
      setStartingId(null);
    }
  };

  if (!challenges) {
    return error ? (
      <p className="text-sm text-danger">{error}</p>
    ) : (
      <Loader2 className="h-6 w-6 animate-spin text-brand mx-auto" />
    );
  }

  const visible = language === 'all' ? challenges : challenges.filter((c) => c.language === language);

  return (
    <div className="w-full max-w-3xl mx-auto space-y-6 text-left">
      <div className="space-y-1">
        <h2 className="text-2xl font-bold text-foreground">Practicar</h2>
        <p className="text-sm text-muted-foreground">
          Resolvé retos las veces que quieras y recibí una devolución de la IA. Tus prácticas no las ve ningún reclutador.
        </p>
      </div>

      <div className="flex flex-wrap gap-2" role="group" aria-label="Filtrar por lenguaje">
        <FilterChip active={language === 'all'} onClick={() => setLanguage('all')}>
          Todos
        </FilterChip>
        {languages.map((lang) => (
          <FilterChip key={lang} active={language === lang} onClick={() => setLanguage(lang)}>
            {languageLabel(lang)}
          </FilterChip>
        ))}
      </div>

      {error && <p className="text-sm text-danger">{error}</p>}

      {visible.length === 0 ? (
        <p className="text-sm text-muted-foreground">Todavía no hay retos de práctica cargados.</p>
      ) : (
        <ul className="space-y-3">
          {visible.map((challenge) => (
            <li
              key={challenge.id}
              className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-border bg-surface/40 px-5 py-4"
            >
              <div className="min-w-0 space-y-1">
                <p className="font-semibold text-foreground">{challenge.title}</p>
                <p className="text-xs text-subtle font-mono">
                  {languageLabel(challenge.language)} · {difficultyLabel(challenge.difficulty)}
                </p>
              </div>

              <div className="flex items-center gap-4">
                <p className="text-xs text-muted-foreground">
                  {challenge.attempts === 0
                    ? 'Sin intentos'
                    : `${challenge.attempts} ${challenge.attempts === 1 ? 'intento' : 'intentos'}`}
                  {challenge.bestScore !== null && (
                    <>
                      {' · mejor '}
                      <span className="font-mono text-foreground">{challenge.bestScore}/100</span>
                    </>
                  )}
                </p>
                <button
                  type="button"
                  onClick={() => handlePractice(challenge.id)}
                  disabled={startingId !== null}
                  className="inline-flex items-center gap-1.5 rounded-lg bg-indigo-600 px-3.5 py-1.5 text-sm font-medium text-white transition-colors hover:bg-indigo-500 disabled:opacity-60"
                >
                  {startingId === challenge.id ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <Play className="h-4 w-4" />
                  )}
                  Practicar
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

const FilterChip: React.FC<{ active: boolean; onClick: () => void; children: React.ReactNode }> = ({
  active,
  onClick,
  children,
}) => (
  <button
    type="button"
    onClick={onClick}
    aria-pressed={active}
    className={`rounded-full border px-3 py-1 text-xs transition-colors ${
      active
        ? 'border-indigo-500/50 bg-indigo-500/10 text-brand'
        : 'border-border text-muted-foreground hover:text-foreground'
    }`}
  >
    {children}
  </button>
);
