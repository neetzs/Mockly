import React from 'react';
import {
  Legend,
  PolarAngleAxis,
  PolarGrid,
  PolarRadiusAxis,
  Radar,
  RadarChart,
  ResponsiveContainer,
  Tooltip,
} from 'recharts';
import type { CandidateRanking } from '../types/dashboard.types';
import { useTheme } from '@/features/theme';

// Recharts pone los colores como atributos SVG (stroke="..."), donde las variables CSS no funcionan:
// por eso elegimos la paleta en JS según el tema. Valores de la escala slate/indigo de Tailwind.
const PALETTE = {
  dark: { grid: '#334155', label: '#cbd5e1', average: '#64748b', stroke: '#818cf8', fill: '#6366f1', tooltipBg: '#0f172a', tooltipBorder: '#1e293b', tooltipText: '#e2e8f0' },
  light: { grid: '#cbd5e1', label: '#334155', average: '#94a3b8', stroke: '#4f46e5', fill: '#6366f1', tooltipBg: '#ffffff', tooltipBorder: '#e2e8f0', tooltipText: '#0f172a' },
};

interface Props {
  candidate: CandidateRanking;
  ranking: CandidateRanking[];
}

const average = (values: number[]): number =>
  values.length === 0 ? 0 : Math.round(values.reduce((sum, v) => sum + v, 0) / values.length);

/** Radar de competencias del candidato seleccionado vs. el promedio de la búsqueda. */
export const CandidateRadarChart: React.FC<Props> = ({ candidate, ranking }) => {
  const colors = PALETTE[useTheme().resolvedTheme];
  // Recharts espera un array de "ejes": cada objeto es un vértice del radar con un valor por serie.
  const data = [
    {
      axis: 'Resolución',
      candidato: candidate.problemSolvingScore,
      promedio: average(ranking.map((r) => r.problemSolvingScore)),
    },
    {
      axis: 'Calidad de código',
      candidato: candidate.codeQualityScore,
      promedio: average(ranking.map((r) => r.codeQualityScore)),
    },
    {
      axis: 'Arquitectura',
      candidato: candidate.architectureScore,
      promedio: average(ranking.map((r) => r.architectureScore)),
    },
  ];

  return (
    <section className="rounded-xl border border-border bg-surface/40 p-5 space-y-2">
      <div>
        <h3 className="text-sm font-semibold text-foreground">{candidate.candidateName}</h3>
        <p className="text-xs text-subtle">
          {candidate.challengeTitle} · {candidate.overallScore}/100
        </p>
      </div>

      <div className="h-72">
        <ResponsiveContainer width="100%" height="100%">
          <RadarChart data={data} outerRadius="70%">
            <PolarGrid stroke={colors.grid} />
            <PolarAngleAxis dataKey="axis" tick={{ fill: colors.label, fontSize: 12 }} />
            <PolarRadiusAxis domain={[0, 100]} tick={false} axisLine={false} />
            <Radar name="Promedio" dataKey="promedio" stroke={colors.average} fill={colors.average} fillOpacity={0.2} />
            <Radar name="Candidato" dataKey="candidato" stroke={colors.stroke} fill={colors.fill} fillOpacity={0.45} />
            <Tooltip
              contentStyle={{ background: colors.tooltipBg, border: `1px solid ${colors.tooltipBorder}`, borderRadius: 8, fontSize: 12 }}
              labelStyle={{ color: colors.tooltipText }}
            />
            <Legend wrapperStyle={{ fontSize: 12 }} />
          </RadarChart>
        </ResponsiveContainer>
      </div>
    </section>
  );
};
