import { jest } from '@jest/globals';

export const mockEmployeesService = {
    getEmployees: jest.fn(),
    getEmployeeById: jest.fn(),
    createEmployee: jest.fn(),
    updateEmployee: jest.fn(),
    deleteEmployee: jest.fn(),
};

// Mock das respostas padrão
export const mockEmployeeData = {
    employee: {
        id: '1',
        firstName: 'João',
        lastName: 'Silva',
        email: 'joao.silva@example.com',
        phoneNumber: '(11) 99999-9999',
        dateOfBirth: '1990-01-01',
        jobTitle: {
            id: '1',
            name: 'Desenvolvedor'
        },
        department: {
            id: '1',
            name: 'TI'
        }
    },
    employees: [
        {
            id: '1',
            firstName: 'João',
            lastName: 'Silva',
            email: 'joao.silva@example.com',
            phoneNumber: '(11) 99999-9999',
            dateOfBirth: '1990-01-01',
            jobTitle: {
                id: '1',
                name: 'Desenvolvedor'
            },
            department: {
                id: '1',
                name: 'TI'
            }
        },
        {
            id: '2',
            firstName: 'Maria',
            lastName: 'Santos',
            email: 'maria.santos@example.com',
            phoneNumber: '(11) 88888-8888',
            dateOfBirth: '1985-05-15',
            jobTitle: {
                id: '2',
                name: 'Analista'
            },
            department: {
                id: '2',
                name: 'RH'
            }
        }
    ],
    createRequest: {
        firstName: 'João',
        lastName: 'Silva',
        email: 'joao.silva@example.com',
        phoneNumber: '(11) 99999-9999',
        dateOfBirth: '1990-01-01',
        jobTitleId: '1'
    },
    updateRequest: {
        firstName: 'João',
        lastName: 'Silva',
        email: 'joao.silva@example.com',
        phoneNumber: '(11) 99999-9999',
        dateOfBirth: '1990-01-01',
        jobTitleId: '1'
    }
};

// Configurar mocks padrão
mockEmployeesService.getEmployees.mockResolvedValue(mockEmployeeData.employees);
mockEmployeesService.getEmployeeById.mockResolvedValue(mockEmployeeData.employee);
mockEmployeesService.createEmployee.mockResolvedValue(mockEmployeeData.employee);
mockEmployeesService.updateEmployee.mockResolvedValue(mockEmployeeData.employee);
mockEmployeesService.deleteEmployee.mockResolvedValue({ success: true });

export default mockEmployeesService;
