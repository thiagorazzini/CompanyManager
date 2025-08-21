import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import DepartmentsEditPage from './DepartmentsEditPage';
import { useAuth } from '@hooks/useAuth';
import departmentsService from '@services/departments/departmentsService';

// Mock do hook useAuth
jest.mock('@hooks/useAuth');
const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

// Mock do departmentsService
jest.mock('@services/departments/departmentsService');
const mockDepartmentsService = departmentsService as jest.Mocked<typeof departmentsService>;

// Mock do react-router-dom para useNavigate e useParams
const mockNavigate = jest.fn();
const mockParams = { id: 'dept-1' };

jest.mock('react-router-dom', () => ({
    ...jest.requireActual('react-router-dom'),
    useNavigate: () => mockNavigate,
    useParams: () => mockParams,
}));

// Mock do react-hot-toast
jest.mock('react-hot-toast', () => ({
    __esModule: true,
    default: {
        success: jest.fn(),
        error: jest.fn(),
        warning: jest.fn(),
        info: jest.fn(),
    },
    toast: {
        success: jest.fn(),
        error: jest.fn(),
        warning: jest.fn(),
        info: jest.fn(),
    },
}));

const renderDepartmentsEditPage = () => {
    return render(
        <BrowserRouter>
            <DepartmentsEditPage />
        </BrowserRouter>
    );
};

describe('DepartmentsEditPage', () => {
    const mockUser = {
        id: 'user-123',
        username: 'admin',
        email: 'admin@companymanager.com',
    };

    const mockDepartment = {
        id: 'dept-1',
        name: 'Recursos Humanos',
        description: 'Departamento responsável pela gestão de pessoas',
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

        mockDepartmentsService.getDepartmentById.mockResolvedValue(mockDepartment);
        mockDepartmentsService.updateDepartment.mockResolvedValue(mockDepartment);
    });

    it('deve renderizar a página de edição de departamentos', async () => {
        renderDepartmentsEditPage();

        await waitFor(() => {
            expect(screen.getByText('Editar Departamento')).toBeInTheDocument();
            expect(screen.getByText('Atualizar informações do departamento')).toBeInTheDocument();
            expect(screen.getByText('admin')).toBeInTheDocument();
            expect(screen.getByText('admin@companymanager.com')).toBeInTheDocument();
        });
    });

    it('deve carregar os dados do departamento da API', async () => {
        renderDepartmentsEditPage();

        await waitFor(() => {
            expect(mockDepartmentsService.getDepartmentById).toHaveBeenCalledWith('dept-1');
        });

        expect(screen.getByDisplayValue('Recursos Humanos')).toBeInTheDocument();
        expect(screen.getByDisplayValue('Departamento responsável pela gestão de pessoas')).toBeInTheDocument();
    });

    it('deve exibir os campos do formulário pré-preenchidos', async () => {
        renderDepartmentsEditPage();

        await waitFor(() => {
            expect(screen.getByPlaceholderText('Ex: Recursos Humanos')).toBeInTheDocument();
            expect(screen.getByPlaceholderText('Descreva as responsabilidades e funções do departamento')).toBeInTheDocument();
            expect(screen.getByDisplayValue('Recursos Humanos')).toBeInTheDocument();
            expect(screen.getByDisplayValue('Departamento responsável pela gestão de pessoas')).toBeInTheDocument();
        });
    });

    it('deve exibir os botões de ação', async () => {
        renderDepartmentsEditPage();

        await waitFor(() => {
            expect(screen.getByRole('button', { name: 'Cancelar' })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: 'Salvar Alterações' })).toBeInTheDocument();
        });
    });

    it('deve validar campos obrigatórios', async () => {
        renderDepartmentsEditPage();

        await waitFor(() => {
            const nameInput = screen.getByPlaceholderText('Ex: Recursos Humanos');
            const descriptionInput = screen.getByPlaceholderText('Descreva as responsabilidades e funções do departamento');

            // Limpar campos
            fireEvent.change(nameInput, { target: { value: '' } });
            fireEvent.change(descriptionInput, { target: { value: '' } });

            const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
            fireEvent.click(submitButton);
        });

        await waitFor(() => {
            expect(screen.getByText('Nome é obrigatório')).toBeInTheDocument();
            expect(screen.getByText('Descrição é obrigatória')).toBeInTheDocument();
        });
    });

    it('deve limpar erros quando o usuário começar a digitar', async () => {
        renderDepartmentsEditPage();

        await waitFor(() => {
            const nameInput = screen.getByLabelText('Nome do Departamento');

            // Limpar campo
            fireEvent.change(nameInput, { target: { value: '' } });

            const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
            fireEvent.click(submitButton);
        });

        await waitFor(() => {
            expect(screen.getByText('Nome é obrigatório')).toBeInTheDocument();
        });

        const nameInput = screen.getByLabelText('Nome do Departamento *');
        fireEvent.change(nameInput, { target: { value: 'Teste' } });

        await waitFor(() => {
            expect(screen.queryByText('Nome é obrigatório')).not.toBeInTheDocument();
        });
    });

    it('deve atualizar departamento com sucesso e redirecionar', async () => {
        const { toast } = require('react-hot-toast');
        renderDepartmentsEditPage();

        await waitFor(() => {
            const nameInput = screen.getByPlaceholderText('Ex: Recursos Humanos');
            const descriptionInput = screen.getByPlaceholderText('Descreva as responsabilidades e funções do departamento');

            // Alterar valores
            fireEvent.change(nameInput, { target: { value: 'RH Atualizado' } });
            fireEvent.change(descriptionInput, { target: { value: 'Nova descrição do RH' } });

            // Submeter formulário
            const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
            fireEvent.click(submitButton);
        });

        await waitFor(() => {
            expect(mockDepartmentsService.updateDepartment).toHaveBeenCalledWith('dept-1', {
                name: 'RH Atualizado',
                description: 'Nova descrição do RH',
            });
        });

        expect(toast.success).toHaveBeenCalledWith('Departamento atualizado com sucesso!');
        expect(mockNavigate).toHaveBeenCalledWith('/departments');
    });

    it('deve exibir toast de erro quando falhar ao atualizar', async () => {
        const { toast } = require('react-hot-toast');
        mockDepartmentsService.updateDepartment.mockRejectedValue(new Error('Erro na API'));

        renderDepartmentsEditPage();

        await waitFor(() => {
            const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
            fireEvent.click(submitButton);
        });

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao atualizar departamento');
        });
    });

    it('deve exibir toast de erro quando falhar ao carregar departamento', async () => {
        const { toast } = require('react-hot-toast');
        mockDepartmentsService.getDepartmentById.mockRejectedValue(new Error('Erro na API'));

        renderDepartmentsEditPage();

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao carregar departamento');
            expect(mockNavigate).toHaveBeenCalledWith('/departments');
        });
    });

    it('deve redirecionar para /departments ao cancelar', async () => {
        renderDepartmentsEditPage();

        await waitFor(() => {
            const cancelButton = screen.getByRole('button', { name: 'Cancelar' });
            fireEvent.click(cancelButton);
        });

        expect(mockNavigate).toHaveBeenCalledWith('/departments');
    });

    it('deve exibir loading durante a atualização', async () => {
        // Mock de uma promise que não resolve imediatamente
        let resolvePromise: (value: any) => void;
        const pendingPromise = new Promise((resolve) => {
            resolvePromise = resolve;
        });

        mockDepartmentsService.updateDepartment.mockReturnValue(pendingPromise);

        renderDepartmentsEditPage();

        await waitFor(() => {
            const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
            fireEvent.click(submitButton);
        });

        await waitFor(() => {
            expect(screen.getByText('Salvando...')).toBeInTheDocument();
            expect(screen.getByRole('button', { name: 'Salvando...' })).toBeDisabled();
        });

        // Resolver a promise
        resolvePromise!(mockDepartment);
    });

    it('deve exibir mensagem quando departamento não for encontrado', async () => {
        mockDepartmentsService.getDepartmentById.mockResolvedValue(null as any);

        renderDepartmentsEditPage();

        await waitFor(() => {
            expect(screen.getByText('Departamento não encontrado')).toBeInTheDocument();
            expect(screen.getByRole('button', { name: 'Voltar para Departamentos' })).toBeInTheDocument();
        });
    });

    it('deve exibir o botão de logout', async () => {
        renderDepartmentsEditPage();

        await waitFor(() => {
            const logoutButton = screen.getByRole('button', { name: 'Sair' });
            expect(logoutButton).toBeInTheDocument();
        });
    });

    it('deve chamar logout quando o botão Sair for clicado', async () => {
        renderDepartmentsEditPage();

        await waitFor(() => {
            const logoutButton = screen.getByRole('button', { name: 'Sair' });
            fireEvent.click(logoutButton);
        });

        expect(mockLogout).toHaveBeenCalledTimes(1);
    });
});


