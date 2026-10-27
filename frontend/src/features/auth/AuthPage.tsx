import React, { useState } from 'react';
import { useAuth } from './context/useAuth';
import { authService } from './services/auth.service';
import { ShieldCheck, UserCheck, Briefcase, Loader2, AlertCircle, CheckCircle2 } from 'lucide-react';

    export const AuthPage: React.FC = () => {
    const { login } = useAuth();
    const [isLoginMode, setIsLoginMode] = useState(true);
    const [loading, setLoading] = useState(false);
    const [errorMsg, setErrorMsg] = useState<string | null>(null);
    const [successMsg, setSuccessMsg] = useState<string | null>(null);

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [fullName, setFullName] = useState('');
    const [role, setRole] = useState<1 | 2>(1); // 1 = Candidate, 2 = Recruiter

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMsg(null);
        setSuccessMsg(null);
        setLoading(true);

        try {
        if (isLoginMode) {
            const session = await authService.login({ email, password });
            login(session);
            setSuccessMsg(`¡Bienvenido, ${session.fullName}!`);
        } else {
            const session = await authService.register({ email, password, fullName, role });
            login(session);
            setSuccessMsg(`¡Cuenta creada con éxito! Rol asignado: ${session.role}.`);
        }
        } catch (err: unknown) {
        if (err instanceof Error) {
            setErrorMsg(err.message);
        } else {
            setErrorMsg('Error de conexión con el servidor.');
        }
        } finally {
        setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-950 flex flex-col justify-center py-12 px-4 sm:px-6 lg:px-8 text-slate-100 font-sans">
        <div className="sm:mx-auto sm:w-full sm:max-w-md">
            <div className="flex justify-center items-center gap-2 mb-2">
            <ShieldCheck className="h-9 w-9 text-indigo-500" />
            <h1 className="text-3xl font-extrabold tracking-tight text-white">Mockly</h1>
            </div>
            <p className="text-center text-sm text-slate-400">
            Evaluación técnica automatizada con IA para Profesionales IT
            </p>
        </div>

        <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
            <div className="bg-slate-900 py-8 px-6 shadow-2xl border border-slate-800 rounded-2xl sm:px-10">
            <div className="flex rounded-lg bg-slate-950 p-1 mb-6 border border-slate-800">
                <button
                type="button"
                onClick={() => {
                    setIsLoginMode(true);
                    setErrorMsg(null);
                }}
                className={`flex-1 py-2 text-sm font-medium rounded-md transition-all ${
                    isLoginMode ? 'bg-indigo-600 text-white shadow' : 'text-slate-400 hover:text-slate-200'
                }`}
                >
                Iniciar Sesión
                </button>
                <button
                type="button"
                onClick={() => {
                    setIsLoginMode(false);
                    setErrorMsg(null);
                }}
                className={`flex-1 py-2 text-sm font-medium rounded-md transition-all ${
                    !isLoginMode ? 'bg-indigo-600 text-white shadow' : 'text-slate-400 hover:text-slate-200'
                }`}
                >
                Registrarse
                </button>
            </div>

            {errorMsg && (
                <div className="mb-4 p-3 bg-red-950/60 border border-red-800/80 rounded-lg flex items-center gap-2 text-red-300 text-sm">
                <AlertCircle className="h-5 w-5 shrink-0" />
                <span>{errorMsg}</span>
                </div>
            )}

            {successMsg && (
                <div className="mb-4 p-3 bg-emerald-950/60 border border-emerald-800/80 rounded-lg flex items-center gap-2 text-emerald-300 text-sm">
                <CheckCircle2 className="h-5 w-5 shrink-0" />
                <span>{successMsg}</span>
                </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
                {!isLoginMode && (
                <>
                    <div>
                    <label className="block text-sm font-medium text-slate-300">Nombre Completo</label>
                    <input
                        type="text"
                        required
                        value={fullName}
                        onChange={(e) => setFullName(e.target.value)}
                        placeholder="Matías Ramírez"
                        className="mt-1 block w-full rounded-md bg-slate-950 border border-slate-800 px-3 py-2 text-white placeholder-slate-500 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 text-sm"
                    />
                    </div>

                    <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1">Rol</label>
                    <div className="grid grid-cols-2 gap-3">
                        <button
                        type="button"
                        onClick={() => setRole(1)}
                        className={`flex items-center justify-center gap-2 py-2 px-3 rounded-lg border text-sm font-medium transition-all ${
                            role === 1
                            ? 'border-indigo-500 bg-indigo-950/40 text-indigo-300'
                            : 'border-slate-800 bg-slate-950 text-slate-400 hover:border-slate-700'
                        }`}
                        >
                        <UserCheck className="h-4 w-4" />
                        Candidato
                        </button>
                        <button
                        type="button"
                        onClick={() => setRole(2)}
                        className={`flex items-center justify-center gap-2 py-2 px-3 rounded-lg border text-sm font-medium transition-all ${
                            role === 2
                            ? 'border-indigo-500 bg-indigo-950/40 text-indigo-300'
                            : 'border-slate-800 bg-slate-950 text-slate-400 hover:border-slate-700'
                        }`}
                        >
                        <Briefcase className="h-4 w-4" />
                        Reclutador
                        </button>
                    </div>
                    </div>
                </>
                )}

                <div>
                <label className="block text-sm font-medium text-slate-300">Correo Electrónico</label>
                <input
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="candidate@devlights.com"
                    className="mt-1 block w-full rounded-md bg-slate-950 border border-slate-800 px-3 py-2 text-white placeholder-slate-500 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 text-sm"
                />
                </div>

                <div>
                <label className="block text-sm font-medium text-slate-300">Contraseña</label>
                <input
                    type="password"
                    required
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="••••••••"
                    className="mt-1 block w-full rounded-md bg-slate-950 border border-slate-800 px-3 py-2 text-white placeholder-slate-500 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 text-sm"
                />
                </div>

                <button
                type="submit"
                disabled={loading}
                className="w-full mt-2 flex justify-center items-center py-2.5 px-4 rounded-md text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-colors disabled:opacity-50"
                >
                {loading ? (
                    <>
                    <Loader2 className="animate-spin -ml-1 mr-2 h-4 w-4" />
                    Procesando...
                    </>
                ) : isLoginMode ? (
                    'Iniciar Sesión'
                ) : (
                    'Registrarse'
                )}
                </button>
            </form>
            </div>
        </div>
        </div>
    );
    };