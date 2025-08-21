import httpClient from '../api/httpClient';

interface LoginCredentials {
    email: string;
    password: string;
}

interface AuthResponse {
    message: string;
    accessToken: string;
    refreshToken?: string;
    expiresAt: string;
    tokenType: string;
    user?: {
        email: string;
        firstName?: string;
        lastName?: string;
    };
}

class AuthService {
    async login(credentials: LoginCredentials): Promise<AuthResponse> {
        const response = await httpClient.post<AuthResponse>('/v1/auth/login', credentials);
        return response.data;
    }

    async logout(): Promise<void> {
        await httpClient.post('/v1/auth/logout');
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
    }

    async refreshToken(): Promise<{ accessToken: string }> {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) {
            throw new Error('No refresh token available');
        }

        const response = await httpClient.post<{ accessToken: string }>('/v1/auth/refresh', { refreshToken });
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
