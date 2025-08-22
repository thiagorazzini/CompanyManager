import httpClient from '../api/httpClient';

export interface JobTitle {
    id: string;
    name: string;
    hierarchyLevel: number;
    description?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
    employeeCount: number;
}

export interface JobTitleListResponse {
    items: JobTitle[];
    total: number;
    page: number;
    pageSize: number;
    hasNext: boolean;
    hasPrev: boolean;
}

export interface JobTitleListRequest {
    page?: number;
    pageSize?: number;
    name?: string;
    hierarchyLevel?: number;
    isActive?: boolean;
}

class JobTitlesService {
    private readonly baseUrl = '/v1/jobtitles';

    async getJobTitles(params?: JobTitleListRequest): Promise<JobTitleListResponse> {
        try {
            const response = await httpClient.get<JobTitleListResponse>(this.baseUrl, { params });
            return response.data;
        } catch (error) {
            throw new Error('Erro ao carregar cargos');
        }
    }

    async getJobTitleById(id: string): Promise<JobTitle> {
        try {
            const response = await httpClient.get<JobTitle>(`${this.baseUrl}/${id}`);
            return response.data;
        } catch (error) {
            throw new Error('Erro ao carregar cargo');
        }
    }

    async getAvailableForCreation(): Promise<JobTitle[]> {
        try {
            const response = await httpClient.get<JobTitle[]>(`${this.baseUrl}/available-for-creation`);
            return response.data;
        } catch (error) {
            throw new Error('Erro ao carregar cargos disponíveis');
        }
    }
}

export default new JobTitlesService();


