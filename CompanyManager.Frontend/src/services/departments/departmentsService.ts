import httpClient from '../api/httpClient';

export interface Department {
    id: string;
    name: string;
    description: string | null;
    createdAt: string;
    updatedAt: string | null;
}

export interface DepartmentListResponse {
    items: Department[];
    pagination: {
        page: number;
        pageSize: number;
        total: number;
        totalPages: number;
    };
}

export interface CreateDepartmentRequest {
    name: string;
    description?: string;
}

export interface UpdateDepartmentRequest {
    name: string;
    description?: string;
}

class DepartmentsService {
    private readonly baseUrl = '/v1/departments';

    async getDepartments(): Promise<Department[]> {
        try {
            const response = await httpClient.get<DepartmentListResponse>(this.baseUrl);
            return response.data.items;
        } catch (error) {
            throw new Error('Erro ao carregar departamentos');
        }
    }

    async getDepartmentById(id: string): Promise<Department> {
        try {
            const response = await httpClient.get<Department>(`${this.baseUrl}/${id}`);
            return response.data;
        } catch (error) {
            throw new Error('Erro ao carregar departamento');
        }
    }

    async createDepartment(data: CreateDepartmentRequest): Promise<Department> {
        try {
            const response = await httpClient.post<Department>(this.baseUrl, data);
            return response.data;
        } catch (error) {
            throw new Error('Erro ao criar departamento');
        }
    }

    async updateDepartment(id: string, data: UpdateDepartmentRequest): Promise<Department> {
        try {
            const response = await httpClient.put<Department>(`${this.baseUrl}/${id}`, data);
            return response.data;
        } catch (error) {
            throw new Error('Erro ao atualizar departamento');
        }
    }

    async deleteDepartment(id: string): Promise<{ success: boolean }> {
        try {
            await httpClient.delete(`${this.baseUrl}/${id}`);
            return { success: true };
        } catch (error) {
            throw new Error('Erro ao remover departamento');
        }
    }
}

export default new DepartmentsService();


