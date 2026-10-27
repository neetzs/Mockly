export type UserRole = 'Candidate' | 'Recruiter';

export interface UserSession {
    id: string;
    email: string;
    fullName: string;
    role: UserRole;
    token: string;
    }

    export interface RegisterPayload {
    email: string;
    password: string;
    fullName: string;
    role: 1 | 2; // 1 = Candidate, 2 = Recruiter
    }

    export interface LoginPayload {
    email: string;
    password: string;
}