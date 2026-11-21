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

interface Props {
  candidate: CandidateRanking;
  ranking: CandidateRanking[];
}

const average = (values: number[]): number =>
  values.length === 0 ? 0 : Math.round(values.reduce((sum, v) => sum + v, 0) / values.length);

/** Radar de competencias del candidato seleccionado vs. el promedio de la búsqueda. */
export const CandidateRadarChart: React.FC<Props> = ({ candidate, ranking }) => {
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
    <section className="rounded-xl border border-slate-800 bg-slate-900/40 p-5 space-y-2">
      <div>
        <h3 className="text-sm font-semibold text-slate-200">{candidate.candidateName}</h3>
        <p className="text-xs text-slate-500">
          {candidate.challengeTitle} · {candidate.overallScore}/100
        </p>
      </div>

      <div className="h-72">
        <ResponsiveContainer width="100%" height="100%">
          <RadarChart data={data} outerRadius="70%">
            <PolarGrid stroke="#334155" />
            <PolarAngleAxis dataKey="axis" tick={{ fill: '#cbd5e1', fontSize: 12 }} />
            <PolarRadiusAxis domain={[0, 100]} tick={false} axisLine={false} />
            <Radar name="Promedio" dataKey="promedio" stroke="#64748b" fill="#64748b" fillOpacity={0.2} />
            <Radar name="Candidato" dataKey="candidato" stroke="#818cf8" fill="#6366f1" fillOpacity={0.45} />
            <Tooltip
              contentStyle={{ background: '#0f172a', border: '1px solid #1e293b', borderRadius: 8, fontSize: 12 }}
              labelStyle={{ color: '#e2e8f0' }}
            />
            <Legend wrapperStyle={{ fontSize: 12 }} />
          </RadarChart>
        </ResponsiveContainer>
      </div>
    </section>
  );
};
