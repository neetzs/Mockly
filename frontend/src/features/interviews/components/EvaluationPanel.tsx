import React from 'react';
import { CheckCircle2, Lightbulb } from 'lucide-react';
import type { EvaluationResult } from '../types/interview.types';

/** Color del puntaje: verde ≥ 75, ámbar ≥ 50, rojo debajo. */
const scoreColor = (score: number): string =>
  score >= 75 ? 'text-emerald-400' : score >= 50 ? 'text-amber-400' : 'text-red-400';

const barColor = (score: number): string =>
  score >= 75 ? 'bg-emerald-500' : score >= 50 ? 'bg-amber-500' : 'bg-red-500';

const ScoreBar: React.FC<{ label: string; value: number }> = ({ label, value }) => (
  <div className="space-y-1">
    <div className="flex justify-between text-xs">
      <span className="text-slate-400">{label}</span>
      <span className={`font-mono font-semibold ${scoreColor(value)}`}>{value}</span>
    </div>
    <div className="h-1.5 rounded-full bg-slate-800 overflow-hidden">
      <div className={`h-full rounded-full ${barColor(value)}`} style={{ width: `${value}%` }} />
    </div>
  </div>
);

export const EvaluationPanel: React.FC<{ evaluation: EvaluationResult }> = ({ evaluation }) => (
  <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 space-y-5">
    <div className="flex items-baseline justify-between">
      <h3 className="text-sm font-semibold text-slate-200 uppercase tracking-wider">Evaluación</h3>
      <p className={`text-4xl font-extrabold font-mono ${scoreColor(evaluation.overallScore)}`}>
        {evaluation.overallScore}
        <span className="text-base text-slate-500">/100</span>
      </p>
    </div>

    <div className="space-y-3">
      <ScoreBar label="Resolución del problema" value={evaluation.problemSolvingScore} />
      <ScoreBar label="Calidad de código" value={evaluation.codeQualityScore} />
      <ScoreBar label="Arquitectura" value={evaluation.architectureScore} />
    </div>

    <p className="text-sm text-slate-300 leading-relaxed">{evaluation.summary}</p>

    <div className="grid gap-4 sm:grid-cols-2">
      <div className="space-y-2">
        <h4 className="text-xs font-semibold text-emerald-400 uppercase">Fortalezas</h4>
        <ul className="space-y-1.5">
          {evaluation.strengths.map((item) => (
            <li key={item} className="flex gap-2 text-xs text-slate-300">
              <CheckCircle2 className="h-3.5 w-3.5 mt-0.5 shrink-0 text-emerald-400" />
              {item}
            </li>
          ))}
        </ul>
      </div>
      <div className="space-y-2">
        <h4 className="text-xs font-semibold text-amber-400 uppercase">Para mejorar</h4>
        <ul className="space-y-1.5">
          {evaluation.improvements.map((item) => (
            <li key={item} className="flex gap-2 text-xs text-slate-300">
              <Lightbulb className="h-3.5 w-3.5 mt-0.5 shrink-0 text-amber-400" />
              {item}
            </li>
          ))}
        </ul>
      </div>
    </div>
  </section>
);
