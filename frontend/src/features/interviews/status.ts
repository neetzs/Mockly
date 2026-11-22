import type { InterviewStatus } from './types/interview.types';

/** Texto y colores de cada estado de una entrevista. Lo comparten la vista del candidato y la del reclutador. */
export const STATUS_LABEL: Record<InterviewStatus, { text: string; className: string }> = {
  NotStarted: { text: 'Pendiente', className: 'text-soft border-input bg-muted/60' },
  InProgress: { text: 'En curso', className: 'text-brand border-indigo-500/30 bg-indigo-500/10' },
  Completed: { text: 'Completada', className: 'text-success border-emerald-500/30 bg-emerald-500/10' },
  Failed: { text: 'Fallida', className: 'text-danger border-red-500/30 bg-red-500/10' },
};
