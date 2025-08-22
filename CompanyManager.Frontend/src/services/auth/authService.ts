import httpClient from '../api/httpClient';

interface LoginCredentials {
    email: string;
    password: string;
}

interface AuthResponse {
    message: string;
    accessToken: string;
    expiresAt: string;
    tokenType: string;
    user?: {
        id: string;
        email: string;
        firstName?: string;
        lastName?: string;
    };
}

interface LogoutResponse {
    message: string;
    timestamp: string;
}

interface UserProfileResponse {
    id: string;
    email: string;
    firstName?: string;
    lastName?: string;
    isActive: boolean;
    lastLoginAt: string;
    jobTitle?: {
        id: string;
        name: string;
        hierarchyLevel: number;
        description: string;
    };
    role?: {
        id: string;
        name: string;
        level: string;
    };
}

class AuthService {
    async login(credentials: LoginCredentials): Promise<AuthResponse> {
        const response = await httpClient.post<AuthResponse>('/v1/auth/login', credentials);
        return response.data;
    }

    async logout(): Promise<LogoutResponse> {
        const response = await httpClient.post<LogoutResponse>('/v1/auth/logout');
        localStorage.removeItem('token');
        return response.data;
    }



    async getProfile(): Promise<UserProfileResponse> {
        const response = await httpClient.get<UserProfileResponse>('/v1/auth/profile');
        return response.data;
    }

    isAuthenticated(): boolean {
        return !!localStorage.getItem('token');
    }

    getToken(): string | null {
        return localStorage.getItem('token');
    }
}

export default new AuthService();
