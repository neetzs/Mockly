import React from "react";
import { BrowserRouter, Navigate, Outlet, Route, Routes } from "react-router-dom";
import { AuthProvider, useAuth } from "./features/auth/context";
import { AuthPage } from "./features/auth/AuthPage";
import { MyInterviewsPage } from "./features/interviews/pages/MyInterviewsPage";
import { InterviewRoomPage } from "./features/interviews/pages/InterviewRoomPage";
import { LogOut, Code2, User } from "lucide-react";

/** Layout común (navbar + contenedor). Las páginas se renderizan en <Outlet />. */
const MainLayout: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuth();

  if (!isAuthenticated || !user) {
    return <AuthPage />;
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col font-sans selection:bg-indigo-500 selection:text-white">
      {/* Top Navbar */}
      <header className="border-b border-slate-800 bg-slate-900/60 backdrop-blur-md px-6 py-4 flex items-center justify-between sticky top-0 z-50">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-indigo-600/20 border border-indigo-500/30 rounded-lg">
            <Code2 className="h-5 w-5 text-indigo-400" />
          </div>
          <div>
            <span className="text-lg font-bold tracking-tight text-white flex items-center gap-1.5">
              Mockly <span className="text-indigo-400 text-xs px-1.5 py-0.5 rounded border border-indigo-500/30 bg-indigo-500/10 font-mono">AI</span>
            </span>
          </div>
        </div>

        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2 text-xs bg-slate-900 border border-slate-800 px-3 py-1.5 rounded-full">
            <User className="h-3.5 w-3.5 text-slate-400" />
            <span className="font-medium text-slate-200">{user.fullName}</span>
            <span className="text-slate-500">•</span>
            <span className="text-indigo-400 font-semibold uppercase tracking-wider text-[10px]">
              {user.role}
            </span>
          </div>

          <button
            type="button"
            onClick={logout}
            className="inline-flex items-center gap-1.5 text-xs text-slate-400 hover:text-red-400 transition-colors border border-slate-800 hover:border-red-900/50 bg-slate-900/50 px-3 py-1.5 rounded-lg"
          >
            <LogOut className="h-3.5 w-3.5" />
            <span>Cerrar Sesión</span>
          </button>
        </div>
      </header>

      <main className="flex-1 w-full max-w-7xl mx-auto p-6 md:p-8 flex flex-col">
        <Outlet />
      </main>
    </div>
  );
};

/** Home según el rol: el candidato ve sus entrevistas; el reclutador, su dashboard (pendiente). */
const HomePage: React.FC = () => {
  const { user } = useAuth();

  if (user?.role === "Recruiter") {
    return (
      <div className="text-center space-y-2 py-16">
        <h2 className="text-2xl font-bold text-white">Dashboard de reclutador</h2>
        <p className="text-sm text-slate-400">Próximamente: ranking de candidatos en tiempo real.</p>
      </div>
    );
  }

  return <MyInterviewsPage />;
};

/** La sala es solo para candidatos (el backend igual lo valida con RequireRole). */
const CandidateOnly: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user } = useAuth();
  return user?.role === "Candidate" ? children : <Navigate to="/" replace />;
};

export default function App(): React.JSX.Element {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<MainLayout />}>
            <Route index element={<HomePage />} />
            <Route
              path="interviews/:id"
              element={
                <CandidateOnly>
                  <InterviewRoomPage />
                </CandidateOnly>
              }
            />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
