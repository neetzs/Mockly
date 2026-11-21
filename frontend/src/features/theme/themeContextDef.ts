import { createContext } from 'react';

/** Lo que elige el usuario. "system" sigue la preferencia del sistema operativo. */
export type ThemePreference = 'light' | 'dark' | 'system';
/** El tema que se está mostrando de verdad (system ya resuelto a light o dark). */
export type ResolvedTheme = 'light' | 'dark';

export interface ThemeContextType {
  theme: ThemePreference;
  resolvedTheme: ResolvedTheme;
  setTheme: (theme: ThemePreference) => void;
}

// Misma clave que lee el script inline de index.html (evita el "flash" de tema al cargar).
export const THEME_STORAGE_KEY = 'mockly_theme';

export const ThemeContext = createContext<ThemeContextType | undefined>(undefined);
