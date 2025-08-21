import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import DepartmentsPage from './DepartmentsPage';
import { useAuth } from '@hooks/useAuth';
import * as departmentsService from '@services/departments/departmentsService';

// Mock do hook useAuth
jest.mock('@hooks/useAuth');
const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

// Mock do departmentsService
jest.mock('@services/departments/departmentsService');
const mockDepartmentsService = departmentsService as jest.Mocked<typeof departmentsService>;

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

const renderDepartmentsPage = () => {
    return render(
        <BrowserRouter>
            <DepartmentsPage />
        </BrowserRouter>
    );
};

describe('DepartmentsPage', () => {
    const mockUser = {
        id: 'user-123',
        username: 'admin@companymanager.com',
        email: 'admin@companymanager.com',
    };

    const mockDepartments = [
        {
            id: 'dept-1',
            name: 'Recursos Humanos',
            description: 'Departamento responsável pela gestão de pessoas',
        },
        {
            id: 'dept-2',
            name: 'Tecnologia',
            description: 'Departamento de desenvolvimento e infraestrutura',
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

        mockDepartmentsService.getDepartments.mockResolvedValue(mockDepartments);
        mockDepartmentsService.deleteDepartment.mockResolvedValue({ success: true });
    });

    it('deve renderizar a página de departamentos com informações do usuário', () => {
        renderDepartmentsPage();

        expect(screen.getByText('Departamentos')).toBeInTheDocument();
        expect(screen.getByText('Gerenciamento de departamentos da empresa')).toBeInTheDocument();
        expect(screen.getByText('admin@companymanager.com')).toBeInTheDocument();
    });

    it('deve carregar e exibir a lista de departamentos da API', async () => {
        renderDepartmentsPage();

        await waitFor(() => {
            expect(screen.getByText('Recursos Humanos')).toBeInTheDocument();
            expect(screen.getByText('Tecnologia')).toBeInTheDocument();
            expect(screen.getByText('Departamento responsável pela gestão de pessoas')).toBeInTheDocument();
            expect(screen.getByText('Departamento de desenvolvimento e infraestrutura')).toBeInTheDocument();
        });
    });

    it('deve exibir o botão "Adicionar Departamento"', async () => {
        renderDepartmentsPage();

        const addButton = screen.getByRole('button', { name: '+ Adicionar Departamento' });
        expect(addButton).toBeInTheDocument();
    });

    it('deve redirecionar para /departments/create ao clicar em "Adicionar Departamento"', async () => {
        renderDepartmentsPage();

        const addButton = screen.getByRole('button', { name: '+ Adicionar Departamento' });
        fireEvent.click(addButton);

        expect(mockNavigate).toHaveBeenCalledWith('/departments/create');
    });

    it('deve exibir botões de "Editar" para cada departamento', async () => {
        renderDepartmentsPage();

        await waitFor(() => {
            const editButtons = screen.getAllByRole('button', { name: 'Editar' });
            expect(editButtons).toHaveLength(2);
        });
    });

    it('deve redirecionar para /departments/edit/:id ao clicar em "Editar"', async () => {
        renderDepartmentsPage();

        await waitFor(() => {
            const editButtons = screen.getAllByRole('button', { name: 'Editar' });
            fireEvent.click(editButtons[0]);

            expect(mockNavigate).toHaveBeenCalledWith('/departments/edit/dept-1');
        });
    });

    it('deve exibir botões de "Remover" para cada departamento', async () => {
        renderDepartmentsPage();

        await waitFor(() => {
            const removeButtons = screen.getAllByRole('button', { name: 'Remover' });
            expect(removeButtons).toHaveLength(2);
        });
    });

    it('deve chamar DELETE API e atualizar a lista ao remover departamento', async () => {
        renderDepartmentsPage();

        await waitFor(() => {
            const removeButtons = screen.getAllByRole('button', { name: 'Remover' });
            fireEvent.click(removeButtons[0]);
        });

        expect(mockDepartmentsService.deleteDepartment).toHaveBeenCalledWith('dept-1');

        // Deve recarregar a lista após remoção
        await waitFor(() => {
            expect(mockDepartmentsService.getDepartments).toHaveBeenCalledTimes(2);
        });
    });

    it('deve exibir toast de erro quando a API retorna erro', async () => {
        const { toast } = require('react-hot-toast');
        mockDepartmentsService.getDepartments.mockRejectedValue(new Error('Erro na API'));

        renderDepartmentsPage();

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao carregar departamentos');
        });
    });

    it('deve exibir toast de erro ao falhar ao remover departamento', async () => {
        const { toast } = require('react-hot-toast');
        mockDepartmentsService.deleteDepartment.mockRejectedValue(new Error('Erro ao remover'));

        renderDepartmentsPage();

        await waitFor(() => {
            const removeButtons = screen.getAllByRole('button', { name: 'Remover' });
            fireEvent.click(removeButtons[0]);
        });

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao remover departamento');
        });
    });

    it('deve exibir mensagem quando não há departamentos', async () => {
        mockDepartmentsService.getDepartments.mockResolvedValue([]);

        renderDepartmentsPage();

        await waitFor(() => {
            expect(screen.getByText('Nenhum departamento encontrado')).toBeInTheDocument();
            expect(screen.getByText('Comece adicionando o primeiro departamento à empresa.')).toBeInTheDocument();
        });
    });

    it('deve exibir o botão de logout', () => {
        renderDepartmentsPage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        expect(logoutButton).toBeInTheDocument();
    });

    it('deve chamar logout quando o botão Sair for clicado', () => {
        renderDepartmentsPage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        fireEvent.click(logoutButton);

        expect(mockLogout).toHaveBeenCalledTimes(1);
    });
});


