import { useContext } from 'react';
import { ThemeContext, type ThemeContextType } from './themeContextDef';

export const useTheme = (): ThemeContextType => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme debe ser utilizado dentro de un ThemeProvider');
  }
  return context;
};
