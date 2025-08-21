import { useState, useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import authService from '@services/auth/authService';
import toast from 'react-hot-toast';

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
        id: string;
        email: string;
        firstName?: string;
        lastName?: string;
    };
}

export const useAuth = () => {
    const [isLoading, setIsLoading] = useState(false);
    const [user, setUser] = useState<AuthResponse['user'] | null>(null);
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const navigate = useNavigate();

    // Verificar autenticação ao inicializar
    useEffect(() => {
        checkAuthStatus();
    }, []);

    const checkAuthStatus = useCallback(() => {
        const token = localStorage.getItem('token');
        const hasValidToken = !!token && token !== 'undefined' && token !== 'null';
        setIsAuthenticated(hasValidToken);
        return hasValidToken;
    }, []);

    const login = useCallback(async (credentials: LoginCredentials) => {
        try {
            setIsLoading(true);

            const response = await authService.login(credentials);

            // Salvar tokens no localStorage
            localStorage.setItem('token', response.accessToken);
            if (response.refreshToken) {
                localStorage.setItem('refreshToken', response.refreshToken);
            }

            // Atualizar estado do usuário
            setUser(response.user || null);
            setIsAuthenticated(true);

            // Mostrar toast de sucesso
            toast.success('Login realizado com sucesso!');

            // Redirecionar para dashboard
            navigate('/dashboard');

            return response;
        } catch (error: any) {
            // Tratar diferentes tipos de erro
            let errorMessage = 'Erro ao fazer login';

            if (error.response?.status === 401) {
                errorMessage = 'Credenciais inválidas';
            } else if (error.response?.status === 400) {
                errorMessage = error.response.data?.message || 'Dados inválidos';
            } else if (error.message) {
                errorMessage = error.message;
            }

            // Mostrar toast de erro
            toast.error(errorMessage);

            throw error;
        } finally {
            setIsLoading(false);
        }
    }, [navigate]);

    const logout = useCallback(async () => {
        try {
            await authService.logout();
        } catch (error) {
            console.error('Erro ao fazer logout:', error);
        } finally {
            // Limpar estado local independente do sucesso da API
            setUser(null);
            setIsAuthenticated(false);
            localStorage.removeItem('token');
            localStorage.removeItem('refreshToken');
            navigate('/login');
        }
    }, [navigate]);

    const checkAuth = useCallback(() => {
        return checkAuthStatus();
    }, [checkAuthStatus]);

    const getToken = useCallback(() => {
        return localStorage.getItem('token');
    }, []);

    const hasValidToken = useCallback(() => {
        const token = getToken();
        return !!token && token !== 'undefined' && token !== 'null';
    }, [getToken]);

    return {
        user,
        isLoading,
        isAuthenticated,
        login,
        logout,
        checkAuth,
        getToken,
        hasValidToken,
    };
};
