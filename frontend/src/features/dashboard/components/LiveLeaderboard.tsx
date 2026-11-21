import React from 'react';
import { Trophy } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { languageLabel } from '@/features/interviews/languages';
import type { CandidateRanking } from '../types/dashboard.types';

interface Props {
  ranking: CandidateRanking[];
  selectedId: string | null;
  highlightedId: string | null;
  onSelect: (sessionId: string) => void;
}

const scoreColor = (score: number): string =>
  score >= 75 ? 'text-emerald-400' : score >= 50 ? 'text-amber-400' : 'text-red-400';

const MEDALS = ['text-amber-300', 'text-slate-300', 'text-orange-400'];

/** Tabla de posiciones. La fila recién llegada por SignalR se resalta unos segundos. */
export const LiveLeaderboard: React.FC<Props> = ({ ranking, selectedId, highlightedId, onSelect }) => {
  if (ranking.length === 0) {
    return (
      <p className="rounded-xl border border-slate-800 bg-slate-900/40 p-8 text-center text-sm text-slate-400">
        Todavía no hay entrevistas completadas. Los resultados van a aparecer acá en vivo.
      </p>
    );
  }

  return (
    <div className="rounded-xl border border-slate-800 bg-slate-900/40 overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow className="border-slate-800 hover:bg-transparent">
            <TableHead className="w-12 text-slate-500">#</TableHead>
            <TableHead className="text-slate-500">Candidato</TableHead>
            <TableHead className="text-slate-500 hidden sm:table-cell">Reto</TableHead>
            <TableHead className="text-right text-slate-500">Puntaje</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {ranking.map((row, index) => {
            const isSelected = row.sessionId === selectedId;
            const isNew = row.sessionId === highlightedId;
            return (
              <TableRow
                key={row.sessionId}
                onClick={() => onSelect(row.sessionId)}
                className={[
                  'cursor-pointer border-slate-800 transition-colors duration-700',
                  isNew ? 'bg-emerald-500/15' : isSelected ? 'bg-indigo-500/10' : 'hover:bg-slate-800/40',
                ].join(' ')}
              >
                <TableCell className="font-mono text-slate-400">
                  {index < 3 ? <Trophy className={`h-4 w-4 ${MEDALS[index]}`} /> : index + 1}
                </TableCell>
                <TableCell>
                  <p className="font-medium text-slate-100">
                    {row.candidateName}
                    {isNew && (
                      <span className="ml-2 text-[10px] font-semibold uppercase text-emerald-400">Nuevo</span>
                    )}
                  </p>
                  <p className="text-xs text-slate-500">{row.candidateEmail}</p>
                </TableCell>
                <TableCell className="hidden sm:table-cell text-slate-300">
                  {row.challengeTitle}
                  <span className="ml-1.5 text-xs font-mono text-slate-500">
                    {languageLabel(row.language)} · {row.difficulty}
                  </span>
                </TableCell>
                <TableCell className={`text-right font-mono font-semibold ${scoreColor(row.overallScore)}`}>
                  {row.overallScore}
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
};
