import type { LoginPayload, RegisterPayload, UserSession } from '../types/auth.types';

// Ajusta el puerto si tu Kestrel corre en 5067 o 5000
const API_BASE_URL = 'http://localhost:5000/api/v1/auth';

    export const authService = {
    async register(payload: RegisterPayload): Promise<UserSession> {
        const res = await fetch(`${API_BASE_URL}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        });

        if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.detail || err.title || 'Error al procesar el registro.');
        }

        return res.json();
    },

    async login(payload: LoginPayload): Promise<UserSession> {
        const res = await fetch(`${API_BASE_URL}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        });

        if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.detail || 'Credenciales inválidas.');
        }

        return res.json();
    },

    async getCurrentUser(token: string): Promise<UserSession> {
        const res = await fetch(`${API_BASE_URL}/me`, {
        headers: { Authorization: `Bearer ${token}` },
        });

        if (!res.ok) {
        throw new Error('Token expirado o no autorizado.');
        }

        return res.json();
    },
    };