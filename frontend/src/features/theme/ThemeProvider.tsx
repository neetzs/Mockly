import React, { useEffect, useState } from 'react';
import { THEME_STORAGE_KEY, ThemeContext, type ResolvedTheme, type ThemePreference } from './themeContextDef';

const DARK_QUERY = '(prefers-color-scheme: dark)';

const readStoredPreference = (): ThemePreference => {
  try {
    const stored = localStorage.getItem(THEME_STORAGE_KEY);
    return stored === 'light' || stored === 'dark' ? stored : 'system';
  } catch {
    return 'system'; // localStorage bloqueado (modo privado estricto): seguimos el sistema
  }
};

const systemTheme = (): ResolvedTheme => (window.matchMedia(DARK_QUERY).matches ? 'dark' : 'light');

/**
 * Maneja el tema claro/oscuro de toda la app.
 * Tailwind está configurado con darkMode: "class": alcanza con poner o sacar la clase "dark" en <html>.
 * Los colores salen de variables CSS (index.css) que cambian según esa clase.
 */
export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [theme, setThemeState] = useState<ThemePreference>(readStoredPreference);
  const [system, setSystem] = useState<ResolvedTheme>(systemTheme);

  // Si el usuario eligió "Sistema", reaccionamos en vivo cuando cambia la preferencia del SO.
  useEffect(() => {
    const media = window.matchMedia(DARK_QUERY);
    const onChange = () => setSystem(media.matches ? 'dark' : 'light');
    media.addEventListener('change', onChange);
    return () => media.removeEventListener('change', onChange);
  }, []);

  const resolvedTheme: ResolvedTheme = theme === 'system' ? system : theme;

  // Efecto = sincronizar React con algo externo (el DOM): aplicamos la clase en <html>.
  useEffect(() => {
    const root = document.documentElement;
    root.classList.toggle('dark', resolvedTheme === 'dark');
    root.style.colorScheme = resolvedTheme; // scrollbars e inputs nativos del color correcto
  }, [resolvedTheme]);

  const setTheme = (next: ThemePreference) => {
    try {
      if (next === 'system') localStorage.removeItem(THEME_STORAGE_KEY);
      else localStorage.setItem(THEME_STORAGE_KEY, next);
    } catch {
      // Sin storage el tema igual cambia; solo no se recuerda para la próxima visita.
    }
    setThemeState(next);
  };

  return <ThemeContext.Provider value={{ theme, resolvedTheme, setTheme }}>{children}</ThemeContext.Provider>;
};
