import { AxiosInstance } from 'axios';

export const setupInterceptors = (client: AxiosInstance): void => {
    // Request interceptor
    client.interceptors.request.use(
        (config) => {
            // Add auth token if available
            const token = localStorage.getItem('token');
            if (token) {
                config.headers.Authorization = `Bearer ${token}`;
            }
            return config;
        },
        (error) => {
            return Promise.reject(error);
        }
    );

    // Response interceptor
    client.interceptors.response.use(
        (response) => {
            return response;
        },
        (error) => {
            // Handle common errors (401, 403, 500, etc.)
            if (error.response?.status === 401) {
                // Handle unauthorized
                localStorage.removeItem('token');
                window.location.href = '/login';
            }
            return Promise.reject(error);
        }
    );
};
