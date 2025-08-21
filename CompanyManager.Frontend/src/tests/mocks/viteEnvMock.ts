// Mock para variáveis de ambiente do Vite
export const mockViteEnv = {
    VITE_API_BASE_URL: 'http://localhost:5000/api'
};

// Mock global para import.meta.env
Object.defineProperty(global, 'import', {
    value: {
        meta: {
            env: mockViteEnv
        }
    },
    writable: true
});

// Mock específico para o Jest
if (typeof global !== 'undefined') {
    (global as any).import = {
        meta: {
            env: mockViteEnv
        }
    };
}
