import React from "react";
import { BrowserRouter, Navigate, Outlet, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./features/auth/context";
import { AuthPage } from "./features/auth/AuthPage";
import { LandingPage } from "./features/landing/LandingPage";
import { MyInterviewsPage } from "./features/interviews/pages/MyInterviewsPage";
import { InterviewRoomPage } from "./features/interviews/pages/InterviewRoomPage";
import { RecruiterDashboardPage } from "./features/dashboard/RecruiterDashboardPage";
import { Navbar } from "./components/layout/Navbar";
import { ProtectedRoute } from "./routes/ProtectedRoute";

/** Layout de la app logueada: navbar + contenedor. Las páginas se renderizan en <Outlet />. */
const AppLayout: React.FC = () => (
  <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col selection:bg-indigo-500 selection:text-white">
    <Navbar />
    <main className="flex-1 w-full max-w-7xl mx-auto p-6 md:p-8 flex flex-col">
      <Outlet />
    </main>
  </div>
);

/**
 * Mapa de rutas:
 *   /                    Landing pública
 *   /login, /register    Autenticación
 *   /interviews[/:id]    Candidatos (listado y sala)
 *   /dashboard           Reclutadores (ranking en tiempo real)
 */
export default function App(): React.JSX.Element {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<AuthPage mode="login" />} />
          <Route path="/register" element={<AuthPage mode="register" />} />

          <Route element={<AppLayout />}>
            <Route element={<ProtectedRoute allowedRoles={["Candidate"]} />}>
              <Route path="/interviews" element={<MyInterviewsPage />} />
              <Route path="/interviews/:id" element={<InterviewRoomPage />} />
            </Route>
            <Route element={<ProtectedRoute allowedRoles={["Recruiter"]} />}>
              <Route path="/dashboard" element={<RecruiterDashboardPage />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
