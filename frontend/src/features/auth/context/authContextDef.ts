import { createContext } from 'react';
import type { UserSession } from '../types/auth.types';

    export interface AuthContextType {
    user: UserSession | null;
    token: string | null;
    isAuthenticated: boolean;
    login: (session: UserSession) => void;
    logout: () => void;
    }

export const AuthContext = createContext<AuthContextType | undefined>(undefined);