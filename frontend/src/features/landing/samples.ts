// Ejemplos del hero de la landing: una solución por lenguaje con su evaluación.
// Los retos son los mismos que carga el seeder (backend/.../Seed/DbSeeder.cs).

export interface CodeSampleData {
  language: string;
  challenge: string;
  code: string;
  keywords: string[];
  commentPrefix: string;
  scores: { label: string; value: number }[];
  overall: number;
}

// Mismo ponderado que el motor de IA: 50% resolución, 30% calidad, 20% arquitectura.
const evaluation = (solving: number, quality: number, architecture: number) => ({
  scores: [
    { label: 'Resolución del problema', value: solving },
    { label: 'Calidad de código', value: quality },
    { label: 'Arquitectura', value: architecture },
  ],
  overall: Math.round(solving * 0.5 + quality * 0.3 + architecture * 0.2),
});

export const SAMPLES: CodeSampleData[] = [
  {
    language: 'C#',
    challenge: 'Paréntesis balanceados',
    commentPrefix: '//',
    keywords: ['public', 'static', 'bool', 'string', 'var', 'new', 'char', 'foreach', 'in', 'if', 'else', 'return', 'false'],
    code: `public static bool IsBalanced(string expression)
{
    var stack = new Stack<char>();
    foreach (var c in expression)
    {
        if ("([{".Contains(c)) stack.Push(c);
        else if (")]}".Contains(c) && !Matches(stack, c))
            return false;
    }
    return stack.Count == 0;
}`,
    ...evaluation(92, 84, 78),
  },
  {
    language: 'Python',
    challenge: 'Contar palabras',
    commentPrefix: '#',
    keywords: ['import', 'from', 'def', 'return', 'str', 'int', 'dict'],
    code: `import re
from collections import Counter

def count_words(text: str) -> dict[str, int]:
    # Minúsculas y solo palabras: la puntuación queda afuera
    words = re.findall(r"\\w+", text.lower())
    return dict(Counter(words))`,
    ...evaluation(95, 88, 70),
  },
  {
    language: 'TypeScript',
    challenge: 'Agrupar por propiedad',
    commentPrefix: '//',
    keywords: ['export', 'function', 'extends', 'keyof', 'return', 'const', 'string'],
    code: `export function groupBy<T, K extends keyof T>(items: T[], key: K) {
  return items.reduce<Record<string, T[]>>((groups, item) => {
    const group = String(item[key]);
    (groups[group] ??= []).push(item);
    return groups;
  }, {});
}`,
    ...evaluation(90, 86, 84),
  },
  {
    language: 'Java',
    challenge: 'Detectar anagramas',
    commentPrefix: '//',
    keywords: ['public', 'static', 'boolean', 'int', 'new', 'for', 'char', 'if', 'return', 'false', 'true'],
    code: `public static boolean isAnagram(String a, String b) {
    int[] counts = new int[26];
    for (char c : normalize(a)) counts[c - 'a']++;
    for (char c : normalize(b)) counts[c - 'a']--;
    for (int n : counts)
        if (n != 0) return false;
    return true;
}`,
    ...evaluation(88, 82, 80),
  },
  {
    language: 'SQL',
    challenge: 'Top clientes por facturación',
    commentPrefix: '--',
    keywords: ['SELECT', 'COUNT', 'SUM', 'AS', 'FROM', 'JOIN', 'ON', 'WHERE', 'AND', 'GROUP', 'BY', 'ORDER', 'DESC', 'LIMIT'],
    code: `SELECT c.name, c.country,
       COUNT(o.id)  AS orders,
       SUM(o.total) AS revenue
FROM customers c
JOIN orders o ON o.customer_id = c.id
WHERE o.created_at >= '2025-01-01'
  AND o.created_at <  '2026-01-01'
GROUP BY c.id, c.name, c.country
ORDER BY revenue DESC
LIMIT 5;`,
    ...evaluation(88, 80, 76),
  },
];
