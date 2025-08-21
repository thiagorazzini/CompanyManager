import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import EmployeesPage from './EmployeesPage';
import { useAuth } from '@hooks/useAuth';

// Mock do hook useAuth
jest.mock('@hooks/useAuth');
const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

// Mock do react-hot-toast
jest.mock('react-hot-toast', () => ({
    toast: {
        success: jest.fn(),
        error: jest.fn(),
        warning: jest.fn(),
    },
}));

const renderEmployeesPage = () => {
    return render(
        <BrowserRouter>
            <EmployeesPage />
        </BrowserRouter>
    );
};

describe('EmployeesPage', () => {
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
    });

    it('deve renderizar a página de funcionários com informações do usuário', () => {
        renderEmployeesPage();

        expect(screen.getByText('Funcionários')).toBeInTheDocument();
        expect(screen.getByText('Gerenciamento de funcionários da empresa')).toBeInTheDocument();
        expect(screen.getAllByText('admin@companymanager.com')[0]).toBeInTheDocument();
        expect(screen.getByText('Lista de Funcionários')).toBeInTheDocument();
    });

    it('deve exibir o botão de logout', () => {
        renderEmployeesPage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        expect(logoutButton).toBeInTheDocument();
    });

    it('deve chamar logout quando o botão Sair for clicado', () => {
        renderEmployeesPage();

        const logoutButton = screen.getByRole('button', { name: 'Sair' });
        fireEvent.click(logoutButton);

        expect(mockLogout).toHaveBeenCalledTimes(1);
    });

    it('deve exibir o botão de novo funcionário', () => {
        renderEmployeesPage();

        const newEmployeeButton = screen.getByRole('button', { name: '+ Novo Funcionário' });
        expect(newEmployeeButton).toBeInTheDocument();
    });

    it('deve exibir mensagem quando não há funcionários', () => {
        renderEmployeesPage();

        expect(screen.getByText('Nenhum funcionário encontrado')).toBeInTheDocument();
        expect(screen.getByText('Comece adicionando o primeiro funcionário à empresa.')).toBeInTheDocument();
    });

    it('deve renderizar com usuário sem informações', () => {
        mockUseAuth.mockReturnValue({
            user: null,
            isLoading: false,
            isAuthenticated: true,
            login: jest.fn(),
            logout: mockLogout,
            checkAuth: jest.fn(),
            getToken: jest.fn(),
            hasValidToken: jest.fn(),
        });

        renderEmployeesPage();

        expect(screen.getByText('Usuário')).toBeInTheDocument();
        expect(screen.getByText('email@exemplo.com')).toBeInTheDocument();
    });
});


