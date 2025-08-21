import '@testing-library/jest-dom';
import './mocks/viteEnvMock';

// Mocks dos serviços serão configurados aqui

// Mock do react-router-dom para testes
jest.mock('react-router-dom', () => ({
    ...jest.requireActual('react-router-dom'),
    useNavigate: () => jest.fn(),
    useLocation: () => ({ pathname: '/' }),
}));

// Mock do react-hot-toast para testes - CORRIGIDO
jest.mock('react-hot-toast', () => {
    const mockToast = {
        success: jest.fn(),
        error: jest.fn(),
        warning: jest.fn(),
        info: jest.fn(),
        dismiss: jest.fn(),
        loading: jest.fn(),
        promise: jest.fn(),
    };

    return {
        __esModule: true,
        default: mockToast,
        toast: mockToast,
        ...mockToast,
    };
});

// Mock do localStorage para testes
const localStorageMock = {
    getItem: jest.fn(),
    setItem: jest.fn(),
    removeItem: jest.fn(),
    clear: jest.fn(),
};
Object.defineProperty(window, 'localStorage', {
    value: localStorageMock,
});

// Mock do @heroicons/react
jest.mock('@heroicons/react/24/outline', () => ({
    XMarkIcon: () => 'X',
}));

// Mock do axios/httpClient
jest.mock('axios', () => ({
    create: jest.fn(() => ({
        get: jest.fn(),
        post: jest.fn(),
        put: jest.fn(),
        delete: jest.fn(),
    })),
    default: {
        create: jest.fn(() => ({
            get: jest.fn(),
            post: jest.fn(),
            put: jest.fn(),
            delete: jest.fn(),
        })),
    },
}));

// CORREÇÃO: Configurar mocks dos serviços usando require em vez de import
// Isso garante que os mocks sejam aplicados corretamente no ambiente CommonJS

// Mock global dos serviços - CORRIGIDO
jest.mock('../services/auth/authService', () => ({
    __esModule: true,
    default: {
        login: jest.fn(),
        logout: jest.fn(),
        refreshToken: jest.fn(),
        isAuthenticated: jest.fn(),
        getToken: jest.fn(),
        getCurrentUser: jest.fn(),
    }
}));

jest.mock('../services/departments/departmentsService', () => ({
    __esModule: true,
    default: {
        getDepartments: jest.fn(),
        getDepartmentById: jest.fn(),
        createDepartment: jest.fn(),
        updateDepartment: jest.fn(),
        deleteDepartment: jest.fn(),
    }
}));

jest.mock('../services/employees/employeesService', () => ({
    __esModule: true,
    default: {
        getEmployees: jest.fn(),
        getEmployeeById: jest.fn(),
        createEmployee: jest.fn(),
        updateEmployee: jest.fn(),
        deleteEmployee: jest.fn(),
    }
}));

jest.mock('../services/jobTitles/jobTitlesService', () => ({
    __esModule: true,
    default: {
        getJobTitles: jest.fn(),
    }
}));

// Mock do httpClient
jest.mock('../services/api/httpClient', () => ({
    __esModule: true,
    default: {
        get: jest.fn(),
        post: jest.fn(),
        put: jest.fn(),
        delete: jest.fn(),
    }
}));
