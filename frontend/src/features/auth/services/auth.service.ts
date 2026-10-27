import type { LoginPayload, RegisterPayload, UserSession } from '../types/auth.types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5067/api/v1';
const AUTH_URL = `${BASE_URL}/auth`;

    export const authService = {
    async register(payload: RegisterPayload): Promise<UserSession> {
        const res = await fetch(`${AUTH_URL}/register`, {
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
        const res = await fetch(`${AUTH_URL}/login`, {
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
        const res = await fetch(`${AUTH_URL}/me`, {
        headers: { Authorization: `Bearer ${token}` },
        });

        if (!res.ok) {
        throw new Error('Token inválido o expirado.');
        }

        return res.json();
    },
    };