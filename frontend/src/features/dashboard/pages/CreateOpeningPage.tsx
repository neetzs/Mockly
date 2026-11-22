import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ArrowLeft, Check, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/features/auth/context';
import { ApiError } from '@/features/interviews/services/interview.service';
import { difficultyLabel, languageLabel } from '@/features/interviews/languages';
import { recruiterService } from '../services/recruiter.service';
import type { LibraryChallenge } from '../types/dashboard.types';

// Mismo tope que JobOpening.MaxChallenges en el backend (el backend lo vuelve a validar).
const MAX_CHALLENGES = 5;

/** Alta de una búsqueda: título, descripción y los retos de la biblioteca que van a resolver los candidatos. */
export const CreateOpeningPage: React.FC = () => {
  const { token, logout } = useAuth();
  const navigate = useNavigate();

  const [library, setLibrary] = useState<LibraryChallenge[] | null>(null);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [selected, setSelected] = useState<string[]>([]);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) return;
    const controller = new AbortController();
    recruiterService
      .getLibrary(token, controller.signal)
      .then(setLibrary)
      .catch((err: unknown) => {
        if (controller.signal.aborted) return;
        if (err instanceof ApiError && err.status === 401) logout();
        setError(err instanceof Error ? err.message : 'No se pudo cargar la biblioteca.');
      });
    return () => controller.abort();
  }, [token, logout]);

  // Mantiene el orden en que el reclutador los fue eligiendo: es el orden de la búsqueda.
  const toggle = (id: string) =>
    setSelected((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!token) return;
    setIsSaving(true);
    setError(null);
    try {
      const opening = await recruiterService.createOpening(
        { title: title.trim(), description: description.trim(), challengeIds: selected },
        token,
      );
      navigate(`/dashboard/openings/${opening.id}`);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) logout();
      setError(err instanceof Error ? err.message : 'No se pudo crear la búsqueda.');
      setIsSaving(false);
    }
  };

  const limitReached = selected.length >= MAX_CHALLENGES;

  return (
    <form onSubmit={handleSubmit} className="w-full max-w-3xl mx-auto space-y-6 text-left">
      <Link to="/dashboard" className="inline-flex items-center gap-1.5 text-xs text-muted-foreground hover:text-foreground">
        <ArrowLeft className="h-3.5 w-3.5" />
        Dashboard
      </Link>

      <div className="space-y-1">
        <h2 className="text-2xl font-bold text-foreground">Nueva búsqueda</h2>
        <p className="text-sm text-muted-foreground">
          Elegí los retos que van a resolver los candidatos. Cada uno se copia a la búsqueda, así todos se evalúan con la
          misma consigna.
        </p>
      </div>

      <div className="space-y-4">
        <label className="block space-y-1.5">
          <span className="text-sm font-medium text-foreground">Título del puesto</span>
          <input
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            maxLength={200}
            placeholder="Frontend Developer SSr (React)"
            className="w-full rounded-lg border border-input bg-surface px-3 py-2 text-sm text-foreground placeholder:text-faint focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </label>
        <label className="block space-y-1.5">
          <span className="text-sm font-medium text-foreground">
            Descripción <span className="font-normal text-muted-foreground">(opcional)</span>
          </span>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            maxLength={4000}
            rows={3}
            placeholder="Equipo, stack y qué se busca evaluar."
            className="w-full rounded-lg border border-input bg-surface px-3 py-2 text-sm text-foreground placeholder:text-faint focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </label>
      </div>

      <fieldset className="space-y-3">
        <legend className="flex w-full items-baseline justify-between text-sm font-medium text-foreground">
          Retos
          <span className="font-normal text-muted-foreground">
            {selected.length} de {MAX_CHALLENGES} como máximo
          </span>
        </legend>

        {!library ? (
          <Loader2 className="h-5 w-5 animate-spin text-brand" />
        ) : (
          <ul className="space-y-2">
            {library.map((challenge) => {
              const order = selected.indexOf(challenge.id);
              const isSelected = order !== -1;
              const disabled = !isSelected && limitReached;
              return (
                <li key={challenge.id}>
                  <button
                    type="button"
                    onClick={() => toggle(challenge.id)}
                    disabled={disabled}
                    aria-pressed={isSelected}
                    className={`flex w-full items-start gap-3 rounded-xl border px-4 py-3 text-left transition-colors disabled:opacity-50 ${
                      isSelected
                        ? 'border-indigo-500/50 bg-indigo-500/10'
                        : 'border-border bg-surface/40 hover:border-indigo-500/30'
                    }`}
                  >
                    <span
                      className={`mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-md border text-[11px] font-semibold ${
                        isSelected ? 'border-indigo-500 bg-indigo-600 text-white' : 'border-input'
                      }`}
                    >
                      {/* El número indica el orden de los retos en la búsqueda */}
                      {isSelected ? order + 1 : <Check className="h-3 w-3 opacity-0" />}
                    </span>
                    <span className="min-w-0 space-y-1">
                      <span className="block font-medium text-foreground">{challenge.title}</span>
                      <span className="block text-xs text-subtle font-mono">
                        {languageLabel(challenge.language)} · {difficultyLabel(challenge.difficulty)}
                      </span>
                      <span className="block text-xs text-muted-foreground line-clamp-2">{challenge.description}</span>
                    </span>
                  </button>
                </li>
              );
            })}
          </ul>
        )}
      </fieldset>

      {error && <p className="text-sm text-danger">{error}</p>}

      <Button type="submit" disabled={isSaving || selected.length === 0 || title.trim() === ''}>
        {isSaving && <Loader2 className="h-4 w-4 animate-spin" />}
        Crear búsqueda
      </Button>
    </form>
  );
};
