import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import EmployeeCreatePage from './EmployeeCreatePage';
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

const renderEmployeeCreatePage = () => {
    return render(
        <BrowserRouter>
            <EmployeeCreatePage />
        </BrowserRouter>
    );
};

describe('EmployeeCreatePage', () => {
    const mockUser = {
        id: 'user-123',
        username: 'admin@companymanager.com',
        email: 'admin@companymanager.com',
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

        mockJobTitlesService.getJobTitles.mockResolvedValue(mockJobTitles);
        mockEmployeesService.createEmployee.mockResolvedValue({
            id: 'emp-new',
            firstName: 'Novo',
            lastName: 'Funcionário',
            email: 'novo@empresa.com',
            phoneNumber: '(11) 77777-7777',
            dateOfBirth: '1995-01-01',
            jobTitle: { id: 'job-1', name: 'Desenvolvedor' },
            department: { id: 'dept-1', name: 'Tecnologia' },
        });
    });

    it('deve renderizar a página de criação de funcionários', () => {
        renderEmployeeCreatePage();

        expect(screen.getByText('Criar Funcionário')).toBeInTheDocument();
        expect(screen.getByText('Adicionar novo funcionário à empresa')).toBeInTheDocument();
        expect(screen.getByText('admin@companymanager.com')).toBeInTheDocument();
    });

    it('deve carregar e exibir os job titles no select', async () => {
        renderEmployeeCreatePage();

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

    it('deve exibir todos os campos do formulário', () => {
        renderEmployeeCreatePage();

        expect(screen.getByLabelText('Nome *')).toBeInTheDocument();
        expect(screen.getByLabelText('Sobrenome *')).toBeInTheDocument();
        expect(screen.getByLabelText('Email *')).toBeInTheDocument();
        expect(screen.getByLabelText('Telefone *')).toBeInTheDocument();
        expect(screen.getByLabelText('Data de Nascimento *')).toBeInTheDocument();
        expect(screen.getByLabelText('Cargo *')).toBeInTheDocument();
    });

    it('deve exibir os botões de ação', () => {
        renderEmployeeCreatePage();

        expect(screen.getByRole('button', { name: 'Cancelar' })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Criar Funcionário' })).toBeInTheDocument();
    });

    it('deve validar campos obrigatórios', async () => {
        renderEmployeeCreatePage();

        const submitButton = screen.getByRole('button', { name: 'Criar Funcionário' });
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
        renderEmployeeCreatePage();

        const submitButton = screen.getByRole('button', { name: 'Criar Funcionário' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText('Nome é obrigatório')).toBeInTheDocument();
        });

        const nameInput = screen.getByLabelText('Nome *');
        fireEvent.change(nameInput, { target: { value: 'Teste' } });

        await waitFor(() => {
            expect(screen.queryByText('Nome é obrigatório')).not.toBeInTheDocument();
        });
    });

    it('deve criar funcionário com sucesso e redirecionar', async () => {
        const { toast } = require('react-hot-toast');
        renderEmployeeCreatePage();

        // Aguardar carregamento dos job titles
        await waitFor(() => {
            expect(mockJobTitlesService.getJobTitles).toHaveBeenCalled();
        });

        // Preencher formulário
        const firstNameInput = screen.getByLabelText('Nome *');
        const lastNameInput = screen.getByLabelText('Sobrenome *');
        const emailInput = screen.getByLabelText('Email *');
        const phoneInput = screen.getByLabelText('Telefone *');
        const dateInput = screen.getByLabelText('Data de Nascimento *');
        const jobTitleSelect = screen.getByLabelText('Cargo *');

        fireEvent.change(firstNameInput, { target: { value: 'Novo' } });
        fireEvent.change(lastNameInput, { target: { value: 'Funcionário' } });
        fireEvent.change(emailInput, { target: { value: 'novo@empresa.com' } });
        fireEvent.change(phoneInput, { target: { value: '(11) 77777-7777' } });
        fireEvent.change(dateInput, { target: { value: '1995-01-01' } });
        fireEvent.change(jobTitleSelect, { target: { value: 'job-1' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Criar Funcionário' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(mockEmployeesService.createEmployee).toHaveBeenCalledWith({
                firstName: 'Novo',
                lastName: 'Funcionário',
                email: 'novo@empresa.com',
                phoneNumber: '(11) 77777-7777',
                dateOfBirth: '1995-01-01',
                jobTitleId: 'job-1',
            });
        });

        expect(toast.success).toHaveBeenCalledWith('Funcionário criado com sucesso!');
        expect(mockNavigate).toHaveBeenCalledWith('/employees');
    });

    it('deve exibir toast de erro quando falhar ao criar', async () => {
        const { toast } = require('react-hot-toast');
        mockEmployeesService.createEmployee.mockRejectedValue(new Error('Erro na API'));

        renderEmployeeCreatePage();

        // Aguardar carregamento dos job titles
        await waitFor(() => {
            expect(mockJobTitlesService.getJobTitles).toHaveBeenCalled();
        });

        // Preencher formulário
        const firstNameInput = screen.getByLabelText('Nome *');
        const lastNameInput = screen.getByLabelText('Sobrenome *');
        const emailInput = screen.getByLabelText('Email *');
        const phoneInput = screen.getByLabelText('Telefone *');
        const dateInput = screen.getByLabelText('Data de Nascimento *');
        const jobTitleSelect = screen.getByLabelText('Cargo *');

        fireEvent.change(firstNameInput, { target: { value: 'Novo' } });
        fireEvent.change(lastNameInput, { target: { value: 'Funcionário' } });
        fireEvent.change(emailInput, { target: { value: 'novo@empresa.com' } });
        fireEvent.change(phoneInput, { target: { value: '(11) 77777-7777' } });
        fireEvent.change(dateInput, { target: { value: '1995-01-01' } });
        fireEvent.change(jobTitleSelect, { target: { value: 'job-1' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Criar Funcionário' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao criar funcionário');
        });
    });

    it('deve exibir toast de erro ao falhar ao carregar job titles', async () => {
        const { toast } = require('react-hot-toast');
        mockJobTitlesService.getJobTitles.mockRejectedValue(new Error('Erro ao carregar cargos'));

        renderEmployeeCreatePage();

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith('Erro ao carregar cargos');
        });
    });

    it('deve redirecionar para /employees ao cancelar', () => {
        renderEmployeeCreatePage();

        const cancelButton = screen.getByRole('button', { name: 'Cancelar' });
        fireEvent.click(cancelButton);

        expect(mockNavigate).toHaveBeenCalledWith('/employees');
    });

    it('deve exibir loading durante a criação', async () => {
        // Mock de uma promise que não resolve imediatamente
        let resolvePromise: (value: any) => void;
        const pendingPromise = new Promise((resolve) => {
            resolvePromise = resolve;
        });

        mockEmployeesService.createEmployee.mockReturnValue(pendingPromise);

        renderEmployeeCreatePage();

        // Aguardar carregamento dos job titles
        await waitFor(() => {
            expect(mockJobTitlesService.getJobTitles).toHaveBeenCalled();
        });

        // Preencher formulário
        const firstNameInput = screen.getByLabelText('Nome *');
        const lastNameInput = screen.getByLabelText('Sobrenome *');
        const emailInput = screen.getByLabelText('Email *');
        const phoneInput = screen.getByLabelText('Telefone *');
        const dateInput = screen.getByLabelText('Data de Nascimento *');
        const jobTitleSelect = screen.getByLabelText('Cargo *');

        fireEvent.change(firstNameInput, { target: { value: 'Novo' } });
        fireEvent.change(lastNameInput, { target: { value: 'Funcionário' } });
        fireEvent.change(emailInput, { target: { value: 'novo@empresa.com' } });
        fireEvent.change(phoneInput, { target: { value: '(11) 77777-7777' } });
        fireEvent.change(dateInput, { target: { value: '1995-01-01' } });
        fireEvent.change(jobTitleSelect, { target: { value: 'job-1' } });

        // Submeter formulário
        const submitButton = screen.getByRole('button', { name: 'Criar Funcionário' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText('Criando...')).toBeInTheDocument();
            expect(screen.getByRole('button', { name: 'Criando...' })).toBeDisabled();
        });

        // Resolver a promise
        resolvePromise!({
            id: 'emp-new',
            firstName: 'Novo',
            lastName: 'Funcionário',
            email: 'novo@empresa.com',
            phoneNumber: '(11) 77777-7777',
            dateOfBirth: '1995-01-01',
            jobTitle: { id: 'job-1', name: 'Desenvolvedor' },
            department: { id: 'dept-1', name: 'Tecnologia' },
        });
    });

    it('deve exibir o botão de logout', () => {
        renderEmployeeCreatePage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        expect(logoutButton).toBeInTheDocument();
    });

    it('deve chamar logout quando o botão Sair for clicado', () => {
        renderEmployeeCreatePage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        fireEvent.click(logoutButton);

        expect(mockLogout).toHaveBeenCalledTimes(1);
    });
});


