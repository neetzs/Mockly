import React, { useState } from 'react';
import type { UserSession } from '../types/auth.types';
import { AuthContext } from './authContextDef';

const STORAGE_KEY = 'mockly_auth_session';

    export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<UserSession | null>(() => {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return null;
        try {
        return JSON.parse(raw) as UserSession;
        } catch {
        localStorage.removeItem(STORAGE_KEY);
        return null;
        }
    });

    const login = (session: UserSession) => {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        setUser(session);
    };

    const logout = () => {
        localStorage.removeItem(STORAGE_KEY);
        setUser(null);
    };

    return (
        <AuthContext.Provider
        value={{
            user,
            token: user?.token ?? null,
            isAuthenticated: !!user,
            login,
            logout,
        }}
        >
        {children}
        </AuthContext.Provider>
    );
};