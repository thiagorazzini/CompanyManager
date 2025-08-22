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
    expiresAt: string;
    tokenType: string;
    user?: {
        id: string;
        email: string;
        firstName?: string;
        lastName?: string;
    };
}

interface UserProfile {
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

export const useAuth = () => {
    const [isLoading, setIsLoading] = useState(false);
    const [user, setUser] = useState<AuthResponse['user'] | null>(null);
    const [userProfile, setUserProfile] = useState<UserProfile | null>(null);
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

        // Se tem token válido, carregar profile
        if (hasValidToken) {
            loadUserProfile();
        }

        return hasValidToken;
    }, []);

    const loadUserProfile = useCallback(async () => {
        try {
            const profile = await authService.getProfile();
            setUserProfile(profile);
        } catch (error) {
            // Se falhar ao carregar profile, fazer logout
            logout();
        }
    }, []);

    const login = useCallback(async (credentials: LoginCredentials) => {
        try {
            setIsLoading(true);

            const response = await authService.login(credentials);

            // Salvar token no localStorage
            localStorage.setItem('token', response.accessToken);

            // Atualizar estado do usuário
            if (response.user) {
                setUser({
                    id: response.user.id,
                    email: response.user.email,
                    firstName: response.user.firstName,
                    lastName: response.user.lastName
                });
            } else {
                setUser(null);
            }
            setIsAuthenticated(true);

            // Carregar profile completo do usuário
            await loadUserProfile();

            // Mostrar toast de sucesso usando mensagem do backend
            toast.success(response.message || 'Login successful!');

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
            const response = await authService.logout();

            // Mostrar toast de sucesso usando mensagem do backend
            toast.success(response.message || 'Logged out successfully');
        } catch (error) {
            // Erro no logout - continuar mesmo assim
            toast.error('Error during logout');
        } finally {
            // Limpar estado local independente do sucesso da API
            setUser(null);
            setUserProfile(null);
            setIsAuthenticated(false);
            localStorage.removeItem('token');
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
        userProfile,
        isLoading,
        isAuthenticated,
        login,
        logout,
        checkAuth,
        getToken,
        hasValidToken,
        loadUserProfile,
    };
};
