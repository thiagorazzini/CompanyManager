import httpClient from '../api/httpClient';

export interface Employee {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    documentNumber: string;
    dateOfBirth: string;
    phoneNumbers: string[];
    jobTitleId: string;
    jobTitleName: string;
    departmentId: string;
    departmentName: string;
    roles: string[];
    createdAt: string;
    updatedAt?: string;
}

export interface EmployeeDetail {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    documentNumber: string;
    dateOfBirth?: string;
    phoneNumbers: string[];
    jobTitle: string;
    jobTitleId: string;
    departmentId: string;
    managerId?: string;
    createdAt: string;
    updatedAt?: string;
}

export interface EmployeeListResponse {
    items: Employee[];
    pagination: {
        page: number;
        pageSize: number;
        total: number;
        totalPages: number;
    };
}

export interface CreateEmployeeRequest {
    firstName: string;
    lastName: string;
    email: string;
    documentNumber: string;
    phoneNumbers: string[];
    dateOfBirth: string;
    jobTitleId: string;
    departmentId: string;
    password: string;
    // roleLevel removido - o nível é determinado pelo JobTitle.HierarchyLevel
}

export interface UpdateEmployeeRequest {
    firstName?: string;
    lastName?: string;
    email?: string;
    documentNumber?: string;
    dateOfBirth?: string;
    phoneNumbers?: string[];
    jobTitleId?: string;
    departmentId?: string;
    password?: string;
}

class EmployeesService {
    private readonly baseUrl = '/v1/employees';

    async getEmployees(): Promise<Employee[]> {
        try {
            console.log('🌐 Fazendo requisição para:', this.baseUrl);
            const response = await httpClient.get<EmployeeListResponse>(this.baseUrl);
            console.log('📡 Resposta recebida:', response);
            console.log('📦 Dados extraídos:', response.data);
            console.log('📋 Items extraídos:', response.data.items);
            return response.data.items;
        } catch (error) {
            console.error('❌ Erro ao buscar funcionários:', error);
            throw new Error('Erro ao carregar funcionários');
        }
    }

    async getEmployeeById(id: string): Promise<EmployeeDetail> {
        try {
            const response = await httpClient.get<EmployeeDetail>(`${this.baseUrl}/${id}`);
            return response.data;
        } catch (error) {
            console.error('Erro ao buscar funcionário:', error);
            throw new Error('Erro ao carregar funcionário');
        }
    }

    async createEmployee(data: CreateEmployeeRequest): Promise<Employee> {
        try {
            const response = await httpClient.post<Employee>(this.baseUrl, data);
            return response.data;
        } catch (error) {
            console.error('Erro ao criar funcionário:', error);
            throw new Error('Erro ao criar funcionário');
        }
    }

    async updateEmployee(id: string, data: UpdateEmployeeRequest): Promise<Employee> {
        try {
            const response = await httpClient.put<Employee>(`${this.baseUrl}/${id}`, data);
            return response.data;
        } catch (error) {
            console.error('Erro ao atualizar funcionário:', error);
            throw new Error('Erro ao atualizar funcionário');
        }
    }

    async deleteEmployee(id: string): Promise<{ success: boolean }> {
        try {
            await httpClient.delete(`${this.baseUrl}/${id}`);
            return { success: true };
        } catch (error) {
            console.error('Erro ao deletar funcionário:', error);
            throw new Error('Erro ao deletar funcionário');
        }
    }
}

export default new EmployeesService();


