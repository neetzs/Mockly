import React from 'react';

interface Props {
  code: string;
  keywords: string[];
  commentPrefix: string;
}

const escapeRegex = (text: string) => text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

/**
 * Resaltado de sintaxis mínimo para los ejemplos de la landing (colores del tema oscuro de VS Code).
 * No es un parser: alcanza para comentarios, strings, números y palabras clave.
 * La sala de entrevista usa Monaco, que sí resalta de verdad.
 */
export const CodeSample: React.FC<Props> = ({ code, keywords, commentPrefix }) => {
  // Grupos: 1 comentario · 2 string · 3 número · 4 identificador
  const token = new RegExp(
    `(${escapeRegex(commentPrefix)}.*$)|("(?:[^"\\\\]|\\\\.)*"|'(?:[^'\\\\]|\\\\.)*')|(\\b\\d+\\b)|(\\b[A-Za-z_]\\w*\\b)`,
    'gm',
  );
  const keywordSet = new Set(keywords);

  const parts: React.ReactNode[] = [];
  let last = 0;
  for (const match of code.matchAll(token)) {
    const [text, comment, string, number, word] = match;
    const index = match.index ?? 0;
    if (index > last) parts.push(code.slice(last, index));

    const color = comment
      ? 'text-slate-500'
      : string
        ? 'text-orange-300'
        : number
          ? 'text-lime-300'
          : word && keywordSet.has(word)
            ? 'text-sky-400'
            : null;

    parts.push(
      color ? (
        <span key={index} className={color}>
          {text}
        </span>
      ) : (
        text
      ),
    );
    last = index + text.length;
  }
  parts.push(code.slice(last));

  return (
    <pre className="min-h-[296px] overflow-x-auto px-4 py-4 text-[13px] leading-6 text-slate-300">
      <code>{parts}</code>
    </pre>
  );
};
