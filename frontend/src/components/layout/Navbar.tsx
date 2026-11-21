import React from 'react';
import { Link, NavLink } from 'react-router-dom';
import { Code2, LogOut, User } from 'lucide-react';
import { useAuth } from '@/features/auth/context';
import { homePathFor } from '@/routes/paths';

export const Logo: React.FC = () => (
  <Link to="/" className="flex items-center gap-2.5 rounded-lg focus-visible:outline focus-visible:outline-2 focus-visible:outline-indigo-400">
    <span className="p-1.5 bg-indigo-600/20 border border-indigo-500/30 rounded-lg">
      <Code2 className="h-5 w-5 text-indigo-400" />
    </span>
    <span className="text-lg font-semibold tracking-tight text-white">Mockly</span>
  </Link>
);

/** Barra superior compartida: links públicos para visitantes; panel y cuenta para usuarios logueados. */
export const Navbar: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <header className="sticky top-0 z-50 border-b border-slate-800/80 bg-slate-950/80 backdrop-blur-md">
      <nav className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-4 py-3 sm:px-6">
        <Logo />

        {user ? (
          <div className="flex items-center gap-2 sm:gap-3">
            <NavLink
              to={homePathFor(user.role)}
              className={({ isActive }) =>
                `rounded-lg px-3 py-1.5 text-sm transition-colors ${isActive ? 'text-white' : 'text-slate-400 hover:text-white'}`
              }
            >
              {user.role === 'Recruiter' ? 'Dashboard' : 'Mis entrevistas'}
            </NavLink>
            <span className="hidden sm:flex items-center gap-2 rounded-full border border-slate-800 bg-slate-900 px-3 py-1.5 text-xs">
              <User className="h-3.5 w-3.5 text-slate-400" />
              <span className="font-medium text-slate-200">{user.fullName}</span>
            </span>
            <button
              type="button"
              onClick={logout}
              className="inline-flex items-center gap-1.5 rounded-lg border border-slate-800 bg-slate-900/50 px-3 py-1.5 text-xs text-slate-400 transition-colors hover:border-red-900/50 hover:text-red-400"
            >
              <LogOut className="h-3.5 w-3.5" />
              <span className="hidden sm:inline">Cerrar sesión</span>
            </button>
          </div>
        ) : (
          <div className="flex items-center gap-1 sm:gap-2">
            <a href="/#como-funciona" className="hidden md:block rounded-lg px-3 py-1.5 text-sm text-slate-400 hover:text-white">
              Cómo funciona
            </a>
            <a href="/#planes" className="hidden md:block rounded-lg px-3 py-1.5 text-sm text-slate-400 hover:text-white">
              Planes
            </a>
            <Link to="/login" className="whitespace-nowrap rounded-lg px-2.5 py-1.5 text-sm text-slate-300 hover:text-white sm:px-3">
              Iniciar sesión
            </Link>
            <Link
              to="/register"
              className="whitespace-nowrap rounded-lg bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white transition-colors hover:bg-indigo-500 sm:px-3.5"
            >
              Crear cuenta
            </Link>
          </div>
        )}
      </nav>
    </header>
  );
};
