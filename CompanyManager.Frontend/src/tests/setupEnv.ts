// Configurar variáveis de ambiente para testes
process.env.VITE_API_BASE_URL = 'http://localhost:5000/api';

// Mock global para import.meta.env
Object.defineProperty(global, 'import', {
    value: {
        meta: {
            env: {
                VITE_API_BASE_URL: 'http://localhost:5000/api'
            }
        }
    },
    writable: true
});




