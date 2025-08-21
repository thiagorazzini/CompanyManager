import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';
import { setupInterceptors } from './interceptors';

class HttpClient {
    private client: AxiosInstance;

    constructor() {
        this.client = axios.create({
            baseURL: import.meta.env.VITE_API_BASE_URL,
            timeout: 10000,
        });

        // Setup interceptors for authentication and error handling
        setupInterceptors(this.client);
    }

    public get<T>(url: string, config?: AxiosRequestConfig): Promise<{ data: T }> {
        return this.client.get(url, config);
    }

    public post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<{ data: T }> {
        return this.client.post(url, data, config);
    }

    public put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<{ data: T }> {
        return this.client.put(url, data, config);
    }

    public delete<T>(url: string, config?: AxiosRequestConfig): Promise<{ data: T }> {
        return this.client.delete(url, config);
    }
}

export default new HttpClient();
