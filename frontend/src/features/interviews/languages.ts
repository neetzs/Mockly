// Lenguajes soportados por los retos. Espejo del enum ProgrammingLanguage del backend
// (backend/src/Domain/Enums/ProgrammingLanguage.cs), que llega serializado por nombre.

export type ProgrammingLanguage = 'CSharp' | 'Python' | 'JavaScript' | 'TypeScript' | 'Java' | 'Sql';

/** label: lo que ve el usuario. monaco: id del lenguaje para el resaltado del editor. */
const LANGUAGES: Record<ProgrammingLanguage, { label: string; monaco: string }> = {
  CSharp: { label: 'C#', monaco: 'csharp' },
  Python: { label: 'Python', monaco: 'python' },
  JavaScript: { label: 'JavaScript', monaco: 'javascript' },
  TypeScript: { label: 'TypeScript', monaco: 'typescript' },
  Java: { label: 'Java', monaco: 'java' },
  Sql: { label: 'SQL', monaco: 'sql' },
};

// Si el backend suma un lenguaje antes que el frontend, mostramos el nombre crudo sin resaltado.
export const languageLabel = (language: string): string =>
  LANGUAGES[language as ProgrammingLanguage]?.label ?? language;

// Niveles del enum ChallengeDifficulty del backend, con el nombre que se usa en el mercado local.
const DIFFICULTIES: Record<string, string> = { Junior: 'Junior', MidLevel: 'Semi Senior', Senior: 'Senior' };

export const difficultyLabel = (difficulty: string): string => DIFFICULTIES[difficulty] ?? difficulty;

export const monacoLanguageId = (language: string): string =>
  LANGUAGES[language as ProgrammingLanguage]?.monaco ?? 'plaintext';
