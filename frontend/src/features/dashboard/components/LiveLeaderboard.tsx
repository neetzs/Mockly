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
  score >= 75 ? 'text-success' : score >= 50 ? 'text-warning' : 'text-danger';

const MEDALS = ['text-warning', 'text-soft', 'text-orange-400'];

/** Tabla de posiciones. La fila recién llegada por SignalR se resalta unos segundos. */
export const LiveLeaderboard: React.FC<Props> = ({ ranking, selectedId, highlightedId, onSelect }) => {
  if (ranking.length === 0) {
    return (
      <p className="rounded-xl border border-border bg-surface/40 p-8 text-center text-sm text-muted-foreground">
        Todavía no hay entrevistas completadas. Los resultados van a aparecer acá en vivo.
      </p>
    );
  }

  return (
    <div className="rounded-xl border border-border bg-surface/40 overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow className="border-border hover:bg-transparent">
            <TableHead className="w-12 text-subtle">#</TableHead>
            <TableHead className="text-subtle">Candidato</TableHead>
            <TableHead className="text-subtle hidden sm:table-cell">Reto</TableHead>
            <TableHead className="text-right text-subtle">Puntaje</TableHead>
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
                  'cursor-pointer border-border transition-colors duration-700',
                  isNew ? 'bg-emerald-500/15' : isSelected ? 'bg-indigo-500/10' : 'hover:bg-muted/40',
                ].join(' ')}
              >
                <TableCell className="font-mono text-muted-foreground">
                  {index < 3 ? <Trophy className={`h-4 w-4 ${MEDALS[index]}`} /> : index + 1}
                </TableCell>
                <TableCell>
                  <p className="font-medium text-foreground">
                    {row.candidateName}
                    {isNew && (
                      <span className="ml-2 text-[10px] font-semibold uppercase text-success">Nuevo</span>
                    )}
                  </p>
                  <p className="text-xs text-subtle">{row.candidateEmail}</p>
                </TableCell>
                <TableCell className="hidden sm:table-cell text-soft">
                  {row.challengeTitle}
                  <span className="ml-1.5 text-xs font-mono text-subtle">
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
