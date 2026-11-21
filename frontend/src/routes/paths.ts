import type { UserRole } from '@/features/auth/types/auth.types';

/** Pantalla de inicio de cada rol después del login. */
export const homePathFor = (role: UserRole): string => (role === 'Recruiter' ? '/dashboard' : '/interviews');
