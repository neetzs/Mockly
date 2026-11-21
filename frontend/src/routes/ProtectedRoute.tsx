import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '@/features/auth/context';
import type { UserRole } from '@/features/auth/types/auth.types';
import { homePathFor } from './paths';

/**
 * Guardia de rutas por rol. Se usa como "layout route": envuelve rutas hijas y renderiza <Outlet />.
 * - Sin sesión → /login, recordando a dónde quería ir (state.from) para volver después.
 * - Con otro rol → a su propio inicio (un candidato que entra a /dashboard vuelve a /interviews).
 *
 * Ojo: esto es comodidad de UX. La seguridad real la hace el backend con RequireRole:
 * aunque alguien saltee esta guardia, la API responde 403.
 */
export const ProtectedRoute: React.FC<{ allowedRoles: UserRole[] }> = ({ allowedRoles }) => {
  const { user } = useAuth();
  const location = useLocation();

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  if (!allowedRoles.includes(user.role)) {
    return <Navigate to={homePathFor(user.role)} replace />;
  }

  return <Outlet />;
};
