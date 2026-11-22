import React from "react";
import { BrowserRouter, Navigate, Outlet, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./features/auth/context";
import { ThemeProvider } from "./features/theme";
import { AuthPage } from "./features/auth/AuthPage";
import { LandingPage } from "./features/landing/LandingPage";
import { MyInterviewsPage } from "./features/interviews/pages/MyInterviewsPage";
import { InterviewRoomPage } from "./features/interviews/pages/InterviewRoomPage";
import { RecruiterDashboardPage } from "./features/dashboard/RecruiterDashboardPage";
import { Navbar } from "./components/layout/Navbar";
import { ProtectedRoute } from "./routes/ProtectedRoute";
import { PracticePage } from "./features/practice/pages/PracticePage";
import { CreateOpeningPage } from "./features/dashboard/pages/CreateOpeningPage";
import { OpeningDetailPage } from "./features/dashboard/pages/OpeningDetailPage";
import { SessionReviewPage } from "./features/dashboard/pages/SessionReviewPage";

/** Layout de la app logueada: navbar + contenedor. Las páginas se renderizan en <Outlet />. */
const AppLayout: React.FC = () => (
  <div className="min-h-screen bg-background text-foreground flex flex-col selection:bg-indigo-500 selection:text-white">
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
    <ThemeProvider>
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
              <Route path="/practice" element={<PracticePage />} />
            </Route>
            <Route element={<ProtectedRoute allowedRoles={["Recruiter"]} />}>
              <Route path="/dashboard" element={<RecruiterDashboardPage />} />
              <Route path="/dashboard/openings/new" element={<CreateOpeningPage />} />
              <Route path="/dashboard/openings/:id" element={<OpeningDetailPage />} />
              <Route path="/dashboard/sessions/:id" element={<SessionReviewPage />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
    </ThemeProvider>
  );
}
