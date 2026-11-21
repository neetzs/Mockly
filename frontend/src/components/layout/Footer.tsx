import React from 'react';
import { Logo } from './Navbar';

export const Footer: React.FC = () => (
  <footer className="border-t border-border/80">
    <div className="mx-auto flex max-w-7xl flex-col gap-6 px-4 py-10 sm:flex-row sm:items-center sm:justify-between sm:px-6">
      <div className="space-y-2">
        <Logo />
        <p className="max-w-sm text-sm text-subtle">
          Práctica y evaluación de entrevistas técnicas con IA. Proyecto integrador del bootcamp Devlights.
        </p>
      </div>
      <p className="text-xs text-faint">© {new Date().getFullYear()} Mockly. Planes y precios de demostración.</p>
    </div>
  </footer>
);
