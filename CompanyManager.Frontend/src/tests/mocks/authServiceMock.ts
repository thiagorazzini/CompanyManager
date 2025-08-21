import { jest } from '@jest/globals';

export const mockAuthService = {
    login: jest.fn(),
    logout: jest.fn(),
    refreshToken: jest.fn(),
    isAuthenticated: jest.fn(),
    getToken: jest.fn(),
};

// Mock das respostas padrão
export const mockAuthResponses = {
    login: {
        token: 'mock-jwt-token',
        refreshToken: 'mock-refresh-token',
        user: {
            id: '1',
            username: 'testuser',
            email: 'test@example.com'
        }
    },
    refreshToken: {
        token: 'new-mock-jwt-token'
    }
};

// Configurar mocks padrão
mockAuthService.login.mockResolvedValue(mockAuthResponses.login);
mockAuthService.logout.mockResolvedValue(undefined);
mockAuthService.refreshToken.mockResolvedValue(mockAuthResponses.refreshToken);
mockAuthService.isAuthenticated.mockReturnValue(true);
mockAuthService.getToken.mockReturnValue('mock-jwt-token');

export default mockAuthService;
