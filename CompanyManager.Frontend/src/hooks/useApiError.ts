import { useState, useCallback } from 'react';
import toast from 'react-hot-toast';

interface ApiError {
    message: string;
    code?: string;
    details?: any;
}

export const useApiError = () => {
    const [error, setError] = useState<ApiError | null>(null);

    const handleError = useCallback((error: any) => {
        let apiError: ApiError;

        if (error.response?.data?.message) {
            apiError = {
                message: error.response.data.message,
                code: error.response.data.code,
                details: error.response.data.details,
            };
        } else if (error.message) {
            apiError = {
                message: error.message,
            };
        } else {
            apiError = {
                message: 'An unexpected error occurred',
            };
        }

        setError(apiError);
        toast.error(apiError.message);
    }, []);

    const clearError = useCallback(() => {
        setError(null);
    }, []);

    return {
        error,
        handleError,
        clearError,
    };
};
