import React from "react";
import { AuthProvider, useAuth } from "./features/auth/context";
import { AuthPage } from "./features/auth/AuthPage";
import { LogOut, Code2, Sparkles, User } from "lucide-react";

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

      {/* Main Container / Dashboard temporal */}
      <main className="flex-1 max-w-6xl w-full mx-auto p-6 md:p-8 flex flex-col items-center justify-center text-center space-y-6">

        <h2 className="text-3xl sm:text-4xl font-extrabold tracking-tight text-white max-w-2xl">
          Bienvenido al centro de evaluación técnica,{" "}
          <span className="text-indigo-400">{user.fullName}</span>.
        </h2>

        <p className="text-slate-400 text-sm max-w-lg leading-relaxed">
          Tu sesión con rol <span className="text-slate-200 font-semibold">{user.role}</span> está autenticada y verificada mediante tokens criptográficos JWT. En el siguiente paso montaremos el entorno con Monaco Editor y la evaluación en tiempo real con <code className="text-indigo-300 font-mono text-xs">Microsoft.Extensions.AI</code>.
        </p>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 w-full max-w-md text-left pt-4">
          <div className="p-4 rounded-xl border border-slate-800 bg-slate-900/40 space-y-1">
            <span className="text-xs text-slate-500 font-mono uppercase">Tu Identificador</span>
            <p className="text-xs font-mono text-slate-300 truncate">{user.id}</p>
          </div>
          <div className="p-4 rounded-xl border border-slate-800 bg-slate-900/40 space-y-1">
            <span className="text-xs text-slate-500 font-mono uppercase">Email Registrado</span>
            <p className="text-xs font-mono text-slate-300 truncate">{user.email}</p>
          </div>
        </div>
      </main>
    </div>
  );
};

export default function App(): React.JSX.Element {
  return (
    <AuthProvider>
      <MainLayout />
    </AuthProvider>
  );
}