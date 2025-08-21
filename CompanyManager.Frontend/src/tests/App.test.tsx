import { render, screen } from '@testing-library/react';
import App from '@app/App';

// Mock das rotas para simplificar os testes
jest.mock('@app/routes', () => ({
    __esModule: true,
    default: () => <div data-testid="app-routes">App Routes</div>,
}));

// Mock do react-hot-toast
jest.mock('react-hot-toast', () => ({
    Toaster: ({ children, ...props }: any) => <div data-testid="toaster" {...props}>{children}</div>,
}));

describe('App Component', () => {
    it('renders without crashing', () => {
        render(<App />);
        expect(screen.getByTestId('app-routes')).toBeInTheDocument();
    });

    it('renders with Toaster component', () => {
        render(<App />);
        expect(screen.getByTestId('toaster')).toBeInTheDocument();
    });
});
