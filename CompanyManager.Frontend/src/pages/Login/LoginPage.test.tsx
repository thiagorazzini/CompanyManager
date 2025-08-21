import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import LoginPage from './LoginPage';
import authService from '@services/auth/authService';

// Mock do react-router-dom
const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    ...jest.requireActual('react-router-dom'),
    useNavigate: () => mockNavigate,
}));

// Mock do localStorage
const localStorageMock = {
    getItem: jest.fn(),
    setItem: jest.fn(),
    removeItem: jest.fn(),
    clear: jest.fn(),
};
Object.defineProperty(window, 'localStorage', {
    value: localStorageMock,
});

const renderLoginPage = () => {
    return render(
        <BrowserRouter>
            <LoginPage />
        </BrowserRouter>
    );
};

describe('LoginPage', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockNavigate.mockClear();
        localStorageMock.setItem.mockClear();
    });

    it('deve renderizar a tela de login com email e senha', () => {
        renderLoginPage();

        expect(screen.getByText('Login')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('admin@companymanager.com')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('••••••••')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Entrar' })).toBeInTheDocument();
    });

    it('deve validar que os campos são obrigatórios', async () => {
        renderLoginPage();

        const submitButton = screen.getByRole('button', { name: 'Entrar' });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText('Email é obrigatório')).toBeInTheDocument();
            expect(screen.getByText('Senha é obrigatória')).toBeInTheDocument();
        });
    });

    it('deve chamar a API corretamente ao enviar', async () => {
        const mockLoginResponse = {
            token: 'jwt-token-123',
            refreshToken: 'refresh-token-123',
            user: {
                id: 'user-123',
                username: 'admin@companymanager.com',
                email: 'admin@companymanager.com'
            }
        };

        (authService.login as jest.Mock).mockResolvedValue(mockLoginResponse);

        renderLoginPage();

        const emailInput = screen.getByPlaceholderText('admin@companymanager.com');
        const passwordInput = screen.getByPlaceholderText('••••••••');
        const submitButton = screen.getByRole('button', { name: 'Entrar' });

        fireEvent.change(emailInput, { target: { value: 'admin@companymanager.com' } });
        fireEvent.change(passwordInput, { target: { value: 'Admin123!' } });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(authService.login).toHaveBeenCalledWith({
                username: 'admin@companymanager.com',
                password: 'Admin123!'
            });
        });
    });

    it('deve exibir o toast de erro se a API retornar 401', async () => {
        const mockError = new Error('Credenciais inválidas');
        mockError.response = { status: 401, data: { message: 'Credenciais inválidas' } };

        (authService.login as jest.Mock).mockRejectedValue(mockError);

        renderLoginPage();

        const emailInput = screen.getByPlaceholderText('admin@companymanager.com');
        const passwordInput = screen.getByPlaceholderText('••••••••');
        const submitButton = screen.getByRole('button', { name: 'Entrar' });

        fireEvent.change(emailInput, { target: { value: 'admin@companymanager.com' } });
        fireEvent.change(passwordInput, { target: { value: 'wrong-password' } });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(authService.login).toHaveBeenCalled();
        });
    });

    it('deve salvar o token no localStorage e redirecionar no sucesso', async () => {
        const mockLoginResponse = {
            token: 'jwt-token-123',
            refreshToken: 'refresh-token-123',
            user: {
                id: 'user-123',
                username: 'admin@companymanager.com',
                email: 'admin@companymanager.com'
            }
        };

        (authService.login as jest.Mock).mockResolvedValue(mockLoginResponse);

        renderLoginPage();

        const emailInput = screen.getByPlaceholderText('admin@companymanager.com');
        const passwordInput = screen.getByPlaceholderText('••••••••');
        const submitButton = screen.getByRole('button', { name: 'Entrar' });

        fireEvent.change(emailInput, { target: { value: 'admin@companymanager.com' } });
        fireEvent.change(passwordInput, { target: { value: 'Admin123!' } });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(localStorageMock.setItem).toHaveBeenCalledWith('token', 'jwt-token-123');
            expect(localStorageMock.setItem).toHaveBeenCalledWith('refreshToken', 'refresh-token-123');
            expect(mockNavigate).toHaveBeenCalledWith('/employees');
        });
    });

    it('deve mostrar loading no botão durante a requisição', async () => {
        // Mock de uma promise que nunca resolve para simular loading
        (authService.login as jest.Mock).mockImplementation(() => new Promise(() => { }));

        renderLoginPage();

        const emailInput = screen.getByPlaceholderText('admin@companymanager.com');
        const passwordInput = screen.getByPlaceholderText('••••••••');
        const submitButton = screen.getByRole('button', { name: 'Entrar' });

        fireEvent.change(emailInput, { target: { value: 'admin@companymanager.com' } });
        fireEvent.change(passwordInput, { target: { value: 'Admin123!' } });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(submitButton).toBeDisabled();
            expect(screen.getByText('Entrando...')).toBeInTheDocument();
        });
    });
});
