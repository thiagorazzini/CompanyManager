import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import EmployeesListPage from './EmployeesListPage';
import { useAuth } from '@hooks/useAuth';
import * as employeesService from '@services/employees/employeesService';

// Mock do hook useAuth
jest.mock('@hooks/useAuth');
const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

// Mock do employeesService
jest.mock('@services/employees/employeesService');
const mockEmployeesService = employeesService as jest.Mocked<typeof employeesService>;

// Mock do react-router-dom para useNavigate
const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    ...jest.requireActual('react-router-dom'),
    useNavigate: () => mockNavigate,
}));

// Mock do react-hot-toast
jest.mock('react-hot-toast', () => ({
    toast: {
        success: jest.fn(),
        error: jest.fn(),
    },
}));

const renderEmployeesListPage = () => {
    return render(
        <BrowserRouter>
            <EmployeesListPage />
        </BrowserRouter>
    );
};

describe('EmployeesListPage', () => {
    const mockUser = {
        id: 'user-123',
        username: 'admin@companymanager.com',
        email: 'admin@companymanager.com',
    };

    const mockEmployees = [
        {
            id: 'emp-1',
            firstName: 'João',
            lastName: 'Silva',
            email: 'joao.silva@empresa.com',
            phoneNumber: '(11) 99999-9999',
            dateOfBirth: '1990-01-01',
            jobTitle: {
                id: 'job-1',
                name: 'Desenvolvedor',
            },
            department: {
                id: 'dept-1',
                name: 'Tecnologia',
            },
        },
        {
            id: 'emp-2',
            firstName: 'Maria',
            lastName: 'Santos',
            email: 'maria.santos@empresa.com',
            phoneNumber: '(11) 88888-8888',
            dateOfBirth: '1985-05-15',
            jobTitle: {
                id: 'job-2',
                name: 'Analista',
            },
            department: {
                id: 'dept-2',
                name: 'Recursos Humanos',
            },
        },
    ];

    const mockLogout = jest.fn();

    beforeEach(() => {
        jest.clearAllMocks();
        mockUseAuth.mockReturnValue({
            user: mockUser,
            isLoading: false,
            isAuthenticated: true,
            login: jest.fn(),
            logout: mockLogout,
            checkAuth: jest.fn(),
            getToken: jest.fn(),
            hasValidToken: jest.fn(),
        });

        mockEmployeesService.getEmployees.mockResolvedValue(mockEmployees);
        mockEmployeesService.deleteEmployee.mockResolvedValue({ success: true });
    });

    it('deve renderizar a página de funcionários com informações do usuário', () => {
        renderEmployeesListPage();

        expect(screen.getByText('Funcionários')).toBeInTheDocument();
        expect(screen.getByText('Gerenciamento de funcionários da empresa')).toBeInTheDocument();
        expect(screen.getByText('admin@companymanager.com')).toBeInTheDocument();
    });

    it('deve carregar e exibir a lista de funcionários da API', async () => {
        renderEmployeesListPage();

        await waitFor(() => {
            expect(screen.getByText('João Silva')).toBeInTheDocument();
            expect(screen.getByText('Maria Santos')).toBeInTheDocument();
            expect(screen.getByText('joao.silva@empresa.com')).toBeInTheDocument();
            expect(screen.getByText('maria.santos@empresa.com')).toBeInTheDocument();
            expect(screen.getByText('Desenvolvedor')).toBeInTheDocument();
            expect(screen.getByText('Analista')).toBeInTheDocument();
        });
    });

    it('deve exibir o botão "Criar Novo Funcionário"', async () => {
        renderEmployeesListPage();

        const addButton = screen.getByRole('button', { name: '+ Criar Novo Funcionário' });
        expect(addButton).toBeInTheDocument();
    });

    it('deve redirecionar para /employees/create ao clicar em "Criar Novo Funcionário"', async () => {
        renderEmployeesListPage();

        const addButton = screen.getByRole('button', { name: '+ Criar Novo Funcionário' });
        fireEvent.click(addButton);

        expect(mockNavigate).toHaveBeenCalledWith('/employees/create');
    });

    it('deve exibir botões de "Editar" para cada funcionário', async () => {
        renderEmployeesListPage();

        await waitFor(() => {
            const editButtons = screen.getAllByRole('button', { name: 'Editar' });
            expect(editButtons).toHaveLength(2);
        });
    });

    it('deve redirecionar para /employees/edit/:id ao clicar em "Editar"', async () => {
        renderEmployeesListPage();

        await waitFor(() => {
            const editButtons = screen.getAllByRole('button', { name: 'Editar' });
            fireEvent.click(editButtons[0]);

            expect(mockNavigate).toHaveBeenCalledWith('/employees/edit/emp-1');
        });
    });

    it('deve exibir botões de "Deletar" para cada funcionário', async () => {
        renderEmployeesListPage();

        await waitFor(() => {
            const deleteButtons = screen.getAllByRole('button', { name: 'Deletar' });
            expect(deleteButtons).toHaveLength(2);
        });
    });

    it('deve chamar DELETE API e atualizar a lista ao deletar funcionário', async () => {
        renderEmployeesListPage();

        await waitFor(() => {
            const deleteButtons = screen.getAllByRole('button', { name: 'Deletar' });
            fireEvent.click(deleteButtons[0]);
        });

        expect(mockEmployeesService.deleteEmployee).toHaveBeenCalledWith('emp-1');

        // Deve recarregar a lista após remoção
        await waitFor(() => {
            expect(mockEmployeesService.getEmployees).toHaveBeenCalledTimes(2);
        });
    });

    it('deve exibir toast de erro quando a API retorna erro', async () => {
        const { toast } = require('react-hot-toast');
        mockEmployeesService.getEmployees.mockRejectedValue(new Error('Erro na API'));

        renderEmployeesListPage();

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao carregar funcionários');
        });
    });

    it('deve exibir toast de erro ao falhar ao deletar funcionário', async () => {
        const { toast } = require('react-hot-toast');
        mockEmployeesService.deleteEmployee.mockRejectedValue(new Error('Erro ao deletar'));

        renderEmployeesListPage();

        await waitFor(() => {
            const deleteButtons = screen.getAllByRole('button', { name: 'Deletar' });
            fireEvent.click(deleteButtons[0]);
        });

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao deletar funcionário');
        });
    });

    it('deve exibir mensagem quando não há funcionários', async () => {
        mockEmployeesService.getEmployees.mockResolvedValue([]);

        renderEmployeesListPage();

        await waitFor(() => {
            expect(screen.getByText('Nenhum funcionário encontrado')).toBeInTheDocument();
            expect(screen.getByText('Comece adicionando o primeiro funcionário à empresa.')).toBeInTheDocument();
        });
    });

    it('deve exibir o botão de logout', () => {
        renderEmployeesListPage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        expect(logoutButton).toBeInTheDocument();
    });

    it('deve chamar logout quando o botão Sair for clicado', () => {
        renderEmployeesListPage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        fireEvent.click(logoutButton);

        expect(mockLogout).toHaveBeenCalledTimes(1);
    });

    it('deve exibir loading durante o carregamento inicial', () => {
        // Mock de uma promise que não resolve imediatamente
        let resolvePromise: (value: any) => void;
        const pendingPromise = new Promise((resolve) => {
            resolvePromise = resolve;
        });

        mockEmployeesService.getEmployees.mockReturnValue(pendingPromise);

        renderEmployeesListPage();

        expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();

        // Resolver a promise
        resolvePromise!(mockEmployees);
    });

    it('deve exibir loading durante a exclusão', async () => {
        // Mock de uma promise que não resolve imediatamente
        let resolvePromise: (value: any) => void;
        const pendingPromise = new Promise((resolve) => {
            resolvePromise = resolve;
        });

        mockEmployeesService.deleteEmployee.mockReturnValue(pendingPromise);

        renderEmployeesListPage();

        await waitFor(() => {
            const deleteButtons = screen.getAllByRole('button', { name: 'Deletar' });
            fireEvent.click(deleteButtons[0]);
        });

        await waitFor(() => {
            expect(screen.getByText('Deletando...')).toBeInTheDocument();
        });

        // Resolver a promise
        resolvePromise!({ success: true });
    });
});


