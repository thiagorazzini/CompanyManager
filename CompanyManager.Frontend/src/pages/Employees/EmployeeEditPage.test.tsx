import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import EmployeeEditPage from './EmployeeEditPage';
import { useAuth } from '@hooks/useAuth';
import * as employeesService from '@services/employees/employeesService';
import * as jobTitlesService from '@services/jobTitles/jobTitlesService';

// Mock do hook useAuth
jest.mock('@hooks/useAuth');
const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

// Mock dos serviços
jest.mock('@services/employees/employeesService');
jest.mock('@services/jobTitles/jobTitlesService');
const mockEmployeesService = employeesService as jest.Mocked<typeof employeesService>;
const mockJobTitlesService = jobTitlesService as jest.Mocked<typeof jobTitlesService>;

// Mock do react-router-dom para useNavigate e useParams
const mockNavigate = jest.fn();
const mockParams = { id: 'emp-1' };
jest.mock('react-router-dom', () => ({
    ...jest.requireActual('react-router-dom'),
    useNavigate: () => mockNavigate,
    useParams: () => mockParams,
}));

// Mock do react-hot-toast
jest.mock('react-hot-toast', () => ({
    toast: {
        success: jest.fn(),
        error: jest.fn(),
    },
}));

const renderEmployeeEditPage = () => {
    return render(
        <BrowserRouter>
            <EmployeeEditPage />
        </BrowserRouter>
    );
};

describe('EmployeeEditPage', () => {
    const mockUser = {
        id: 'user-123',
        username: 'admin@companymanager.com',
        email: 'admin@companymanager.com',
    };

    const mockEmployee = {
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
    };

    const mockJobTitles = [
        { id: 'job-1', name: 'Desenvolvedor' },
        { id: 'job-2', name: 'Analista' },
        { id: 'job-3', name: 'Gerente' },
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

        mockEmployeesService.getEmployeeById.mockResolvedValue(mockEmployee);
        mockJobTitlesService.getJobTitles.mockResolvedValue(mockJobTitles);
        mockEmployeesService.updateEmployee.mockResolvedValue(mockEmployee);
    });

    it('deve renderizar a página de edição de funcionários', async () => {
        renderEmployeeEditPage();

        expect(screen.getByText('Editar Funcionário')).toBeInTheDocument();
        expect(screen.getByText('Modificar dados do funcionário')).toBeInTheDocument();
        expect(screen.getByText('admin@companymanager.com')).toBeInTheDocument();
    });

    it('deve carregar os dados do funcionário da API', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            expect(mockEmployeesService.getEmployeeById).toHaveBeenCalledWith('emp-1');
        });
    });

    it('deve carregar e exibir os job titles no select', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            expect(mockJobTitlesService.getJobTitles).toHaveBeenCalled();
        });

        const jobTitleSelect = screen.getByLabelText('Cargo *');
        expect(jobTitleSelect).toBeInTheDocument();

        // Verificar se as opções estão carregadas
        expect(screen.getByText('Desenvolvedor')).toBeInTheDocument();
        expect(screen.getByText('Analista')).toBeInTheDocument();
        expect(screen.getByText('Gerente')).toBeInTheDocument();
    });

    it('deve exibir os campos do formulário pré-preenchidos', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            const firstNameInput = screen.getByLabelText('Nome *') as HTMLInputElement;
            const lastNameInput = screen.getByLabelText('Sobrenome *') as HTMLInputElement;
            const emailInput = screen.getByLabelText('Email *') as HTMLInputElement;
            const phoneInput = screen.getByLabelText('Telefone *') as HTMLInputElement;
            const dateInput = screen.getByLabelText('Data de Nascimento *') as HTMLInputElement;
            const jobTitleSelect = screen.getByLabelText('Cargo *') as HTMLSelectElement;

            expect(firstNameInput.value).toBe('João');
            expect(lastNameInput.value).toBe('Silva');
            expect(emailInput.value).toBe('joao.silva@empresa.com');
            expect(phoneInput.value).toBe('(11) 99999-9999');
            expect(dateInput.value).toBe('1990-01-01');
            expect(jobTitleSelect.value).toBe('job-1');
        });
    });

    it('deve exibir todos os campos do formulário', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            expect(screen.getByLabelText('Nome *')).toBeInTheDocument();
            expect(screen.getByLabelText('Sobrenome *')).toBeInTheDocument();
            expect(screen.getByLabelText('Email *')).toBeInTheDocument();
            expect(screen.getByLabelText('Telefone *')).toBeInTheDocument();
            expect(screen.getByLabelText('Data de Nascimento *')).toBeInTheDocument();
            expect(screen.getByLabelText('Cargo *')).toBeInTheDocument();
        });
    });

    it('deve exibir os botões de ação', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            expect(screen.getByRole('button', { name: 'Cancelar' })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: 'Salvar Alterações' })).toBeInTheDocument();
        });
    });

    it('deve validar campos obrigatórios', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            // Limpar campos obrigatórios
            const firstNameInput = screen.getByLabelText('Nome *');
            const lastNameInput = screen.getByLabelText('Sobrenome *');
            const emailInput = screen.getByLabelText('Email *');
            const phoneInput = screen.getByLabelText('Telefone *');
            const dateInput = screen.getByLabelText('Data de Nascimento *');
            const jobTitleSelect = screen.getByLabelText('Cargo *');

            fireEvent.change(firstNameInput, { target: { value: '' } });
            fireEvent.change(lastNameInput, { target: { value: '' } });
            fireEvent.change(emailInput, { target: { value: '' } });
            fireEvent.change(phoneInput, { target: { value: '' } });
            fireEvent.change(dateInput, { target: { value: '' } });
            fireEvent.change(jobTitleSelect, { target: { value: '' } });
        });

        const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText('Nome é obrigatório')).toBeInTheDocument();
            expect(screen.getByText('Sobrenome é obrigatório')).toBeInTheDocument();
            expect(screen.getByText('Email é obrigatório')).toBeInTheDocument();
            expect(screen.getByText('Telefone é obrigatório')).toBeInTheDocument();
            expect(screen.getByText('Data de nascimento é obrigatória')).toBeInTheDocument();
            expect(screen.getByText('Cargo é obrigatório')).toBeInTheDocument();
        });
    });

    it('deve limpar erros quando o usuário começar a digitar', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            // Limpar campo obrigatório
            const firstNameInput = screen.getByLabelText('Nome *');
            fireEvent.change(firstNameInput, { target: { value: '' } });
        });

        const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText('Nome é obrigatório')).toBeInTheDocument();
        });

        const firstNameInput = screen.getByLabelText('Nome *');
        fireEvent.change(firstNameInput, { target: { value: 'Teste' } });

        await waitFor(() => {
            expect(screen.queryByText('Nome é obrigatório')).not.toBeInTheDocument();
        });
    });

    it('deve atualizar funcionário com sucesso e redirecionar', async () => {
        const { toast } = require('react-hot-toast');
        renderEmployeeEditPage();

        await waitFor(() => {
            expect(mockEmployeesService.getEmployeeById).toHaveBeenCalled();
            expect(mockJobTitlesService.getJobTitles).toHaveBeenCalled();
        });

        // Modificar um campo
        const firstNameInput = screen.getByLabelText('Nome *');
        fireEvent.change(firstNameInput, { target: { value: 'João Atualizado' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(mockEmployeesService.updateEmployee).toHaveBeenCalledWith('emp-1', {
                firstName: 'João Atualizado',
                lastName: 'Silva',
                email: 'joao.silva@empresa.com',
                phoneNumber: '(11) 99999-9999',
                dateOfBirth: '1990-01-01',
                jobTitleId: 'job-1',
            });
        });

        expect(toast.success).toHaveBeenCalledWith('Funcionário atualizado com sucesso!');
        expect(mockNavigate).toHaveBeenCalledWith('/employees');
    });

    it('deve exibir toast de erro quando falhar ao atualizar', async () => {
        const { toast } = require('react-hot-toast');
        mockEmployeesService.updateEmployee.mockRejectedValue(new Error('Erro na API'));

        renderEmployeeEditPage();

        await waitFor(() => {
            expect(mockEmployeesService.getEmployeeById).toHaveBeenCalled();
            expect(mockJobTitlesService.getJobTitles).toHaveBeenCalled();
        });

        // Modificar um campo
        const firstNameInput = screen.getByLabelText('Nome *');
        fireEvent.change(firstNameInput, { target: { value: 'João Atualizado' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao atualizar funcionário');
        });
    });

    it('deve exibir toast de erro quando falhar ao carregar funcionário', async () => {
        const { toast } = require('react-hot-toast');
        mockEmployeesService.getEmployeeById.mockRejectedValue(new Error('Funcionário não encontrado'));

        renderEmployeeEditPage();

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao carregar funcionário');
        });
    });

    it('deve exibir toast de erro ao falhar ao carregar job titles', async () => {
        const { toast } = require('react-hot-toast');
        mockJobTitlesService.getJobTitles.mockRejectedValue(new Error('Erro ao carregar cargos'));

        renderEmployeeEditPage();

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao carregar cargos');
        });
    });

    it('deve redirecionar para /employees ao cancelar', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            const cancelButton = screen.getByRole('button', { name: 'Cancelar' });
            fireEvent.click(cancelButton);
        });

        expect(mockNavigate).toHaveBeenCalledWith('/employees');
    });

    it('deve exibir loading durante a atualização', async () => {
        // Mock de uma promise que não resolve imediatamente
        let resolvePromise: (value: any) => void;
        const pendingPromise = new Promise((resolve) => {
            resolvePromise = resolve;
        });

        mockEmployeesService.updateEmployee.mockReturnValue(pendingPromise);

        renderEmployeeEditPage();

        await waitFor(() => {
            expect(mockEmployeesService.getEmployeeById).toHaveBeenCalled();
            expect(mockJobTitlesService.getJobTitles).toHaveBeenCalled();
        });

        // Modificar um campo
        const firstNameInput = screen.getByLabelText('Nome *');
        fireEvent.change(firstNameInput, { target: { value: 'João Atualizado' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText('Salvando...')).toBeInTheDocument();
            expect(screen.getByRole('button', { name: 'Salvando...' })).toBeDisabled();
        });

        // Resolver a promise
        resolvePromise!(mockEmployee);
    });

    it('deve exibir loading durante o carregamento inicial', () => {
        // Mock de uma promise que não resolve imediatamente
        let resolvePromise: (value: any) => void;
        const pendingPromise = new Promise((resolve) => {
            resolvePromise = resolve;
        });

        mockEmployeesService.getEmployeeById.mockReturnValue(pendingPromise);

        renderEmployeeEditPage();

        expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();

        // Resolver a promise
        resolvePromise!(mockEmployee);
    });

    it('deve exibir mensagem quando funcionário não for encontrado', async () => {
        mockEmployeesService.getEmployeeById.mockRejectedValue(new Error('Funcionário não encontrado'));

        renderEmployeeEditPage();

        await waitFor(() => {
            expect(screen.getByText('Funcionário não encontrado')).toBeInTheDocument();
            expect(screen.getByText('O funcionário que você está tentando editar não foi encontrado.')).toBeInTheDocument();
        });
    });

    it('deve exibir o botão de logout', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            const logoutButton = screen.getByRole('button', { name: 'Sair' });
            expect(logoutButton).toBeInTheDocument();
        });
    });

    it('deve chamar logout quando o botão Sair for clicado', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            const logoutButton = screen.getByRole('button', { name: 'Sair' });
            fireEvent.click(logoutButton);
        });

        expect(mockLogout).toHaveBeenCalledTimes(1);
    });

    it('deve permitir alterar o job title', async () => {
        renderEmployeeEditPage();

        await waitFor(() => {
            expect(mockEmployeesService.getEmployeeById).toHaveBeenCalled();
            expect(mockJobTitlesService.getJobTitles).toHaveBeenCalled();
        });

        // Alterar o cargo
        const jobTitleSelect = screen.getByLabelText('Cargo *');
        fireEvent.change(jobTitleSelect, { target: { value: 'job-2' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Salvar Alterações' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(mockEmployeesService.updateEmployee).toHaveBeenCalledWith('emp-1', {
                firstName: 'João',
                lastName: 'Silva',
                email: 'joao.silva@empresa.com',
                phoneNumber: '(11) 99999-9999',
                dateOfBirth: '1990-01-01',
                jobTitleId: 'job-2',
            });
        });
    });
});


