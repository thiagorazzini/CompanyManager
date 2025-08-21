import { jest } from '@jest/globals';

export const mockDepartmentsService = {
    getDepartments: jest.fn(),
    getDepartmentById: jest.fn(),
    createDepartment: jest.fn(),
    updateDepartment: jest.fn(),
    deleteDepartment: jest.fn(),
};

// Mock das respostas padrão
export const mockDepartmentData = {
    department: {
        id: '1',
        name: 'TI',
        description: 'Departamento de Tecnologia da Informação'
    },
    departments: [
        {
            id: '1',
            name: 'TI',
            description: 'Departamento de Tecnologia da Informação'
        },
        {
            id: '2',
            name: 'RH',
            description: 'Departamento de Recursos Humanos'
        },
        {
            id: '3',
            name: 'Financeiro',
            description: 'Departamento Financeiro'
        }
    ],
    createRequest: {
        name: 'TI',
        description: 'Departamento de Tecnologia da Informação'
    },
    updateRequest: {
        name: 'TI Atualizado',
        description: 'Departamento de TI com nova descrição'
    }
};

// Configurar mocks padrão
mockDepartmentsService.getDepartments.mockResolvedValue(mockDepartmentData.departments);
mockDepartmentsService.getDepartmentById.mockResolvedValue(mockDepartmentData.department);
mockDepartmentsService.createDepartment.mockResolvedValue(mockDepartmentData.department);
mockDepartmentsService.updateDepartment.mockResolvedValue(mockDepartmentData.department);
mockDepartmentsService.deleteDepartment.mockResolvedValue({ success: true });

export default mockDepartmentsService;
