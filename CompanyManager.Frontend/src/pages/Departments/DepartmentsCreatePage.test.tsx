import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import DepartmentsCreatePage from './DepartmentsCreatePage';
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

const renderDepartmentsCreatePage = () => {
    return render(
        <BrowserRouter>
            <DepartmentsCreatePage />
        </BrowserRouter>
    );
};

describe('DepartmentsCreatePage', () => {
    const mockUser = {
        id: 'user-123',
        username: 'admin@companymanager.com',
        email: 'admin@companymanager.com',
    };

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

        mockDepartmentsService.createDepartment.mockResolvedValue({
            id: 'dept-new',
            name: 'Novo Departamento',
            description: 'Descrição do novo departamento',
        });
    });

    it('deve renderizar a página de criação de departamentos', () => {
        renderDepartmentsCreatePage();

        expect(screen.getByText('Criar Departamento')).toBeInTheDocument();
        expect(screen.getByText('Adicionar novo departamento à empresa')).toBeInTheDocument();
        expect(screen.getByText('admin@companymanager.com')).toBeInTheDocument();
    });

    it('deve exibir os campos do formulário', () => {
        renderDepartmentsCreatePage();

        expect(screen.getByLabelText('Nome do Departamento *')).toBeInTheDocument();
        expect(screen.getByLabelText('Descrição *')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('Ex: Recursos Humanos')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('Descreva as responsabilidades e funções do departamento')).toBeInTheDocument();
    });

    it('deve exibir os botões de ação', () => {
        renderDepartmentsCreatePage();

        expect(screen.getByRole('button', { name: 'Cancelar' })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Criar Departamento' })).toBeInTheDocument();
    });

    it('deve validar campos obrigatórios', async () => {
        renderDepartmentsCreatePage();

        const submitButton = screen.getByRole('button', { name: 'Criar Departamento' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText('Nome é obrigatório')).toBeInTheDocument();
            expect(screen.getByText('Descrição é obrigatória')).toBeInTheDocument();
        });
    });

    it('deve limpar erros quando o usuário começar a digitar', async () => {
        renderDepartmentsCreatePage();

        const submitButton = screen.getByRole('button', { name: 'Criar Departamento' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText('Nome é obrigatório')).toBeInTheDocument();
        });

        const nameInput = screen.getByLabelText('Nome do Departamento *');
        fireEvent.change(nameInput, { target: { value: 'Teste' } });

        await waitFor(() => {
            expect(screen.queryByText('Nome é obrigatório')).not.toBeInTheDocument();
        });
    });

    it('deve criar departamento com sucesso e redirecionar', async () => {
        const { toast } = require('react-hot-toast');
        renderDepartmentsCreatePage();

        // Preencher formulário
        const nameInput = screen.getByLabelText('Nome do Departamento *');
        const descriptionInput = screen.getByLabelText('Descrição *');

        fireEvent.change(nameInput, { target: { value: 'Novo Departamento' } });
        fireEvent.change(descriptionInput, { target: { value: 'Descrição do departamento' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Criar Departamento' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(mockDepartmentsService.createDepartment).toHaveBeenCalledWith({
                name: 'Novo Departamento',
                description: 'Descrição do departamento',
            });
        });

        expect(toast.success).toHaveBeenCalledWith('Departamento criado com sucesso!');
        expect(mockNavigate).toHaveBeenCalledWith('/departments');
    });

    it('deve exibir toast de erro quando falhar ao criar', async () => {
        const { toast } = require('react-hot-toast');
        mockDepartmentsService.createDepartment.mockRejectedValue(new Error('Erro na API'));

        renderDepartmentsCreatePage();

        // Preencher formulário
        const nameInput = screen.getByLabelText('Nome do Departamento *');
        const descriptionInput = screen.getByLabelText('Descrição *');

        fireEvent.change(nameInput, { target: { value: 'Novo Departamento' } });
        fireEvent.change(descriptionInput, { target: { value: 'Descrição do departamento' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Criar Departamento' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao criar departamento');
        });
    });

    it('deve redirecionar para /departments ao cancelar', () => {
        renderDepartmentsCreatePage();

        const cancelButton = screen.getByRole('button', { name: 'Cancelar' });
        fireEvent.click(cancelButton);

        expect(mockNavigate).toHaveBeenCalledWith('/departments');
    });

    it('deve exibir loading durante a criação', async () => {
        // Mock de uma promise que não resolve imediatamente
        let resolvePromise: (value: any) => void;
        const pendingPromise = new Promise((resolve) => {
            resolvePromise = resolve;
        });

        mockDepartmentsService.createDepartment.mockReturnValue(pendingPromise);

        renderDepartmentsCreatePage();

        // Preencher formulário
        const nameInput = screen.getByLabelText('Nome do Departamento *');
        const descriptionInput = screen.getByLabelText('Descrição *');

        fireEvent.change(nameInput, { target: { value: 'Novo Departamento' } });
        fireEvent.change(descriptionInput, { target: { value: 'Descrição do departamento' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Criar Departamento' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText('Criando...')).toBeInTheDocument();
            expect(screen.getByRole('button', { name: 'Criando...' })).toBeDisabled();
        });

        // Resolver a promise
        resolvePromise!({
            id: 'dept-new',
            name: 'Novo Departamento',
            description: 'Descrição do departamento',
        });
    });

    it('deve exibir o botão de logout', () => {
        renderDepartmentsCreatePage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        expect(logoutButton).toBeInTheDocument();
    });

    it('deve chamar logout quando o botão Sair for clicado', () => {
        renderDepartmentsCreatePage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        fireEvent.click(logoutButton);

        expect(mockLogout).toHaveBeenCalledTimes(1);
    });
});


