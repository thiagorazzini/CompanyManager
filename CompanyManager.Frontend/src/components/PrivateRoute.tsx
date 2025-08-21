import React, { useEffect } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';
import toast from 'react-hot-toast';

interface PrivateRouteProps {
    children: React.ReactNode;
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ children }) => {
    const { isAuthenticated, hasValidToken } = useAuth();
    const location = useLocation();

    // Verificar tanto o estado quanto o token diretamente
    const isActuallyAuthenticated = isAuthenticated || hasValidToken();

    useEffect(() => {
        if (!isActuallyAuthenticated) {
            toast.error('Você precisa estar logado para acessar esta página', {
                duration: 4000,
                position: 'top-right',
            });
        }
    }, [isActuallyAuthenticated]);

    if (!isActuallyAuthenticated) {
        // Redirecionar para login e salvar a localização atual
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    return <>{children}</>;
};

export default PrivateRoute;
