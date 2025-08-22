import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    server: {
        port: 3000,
    },
    define: {
        'import.meta.env.VITE_API_BASE_URL': JSON.stringify('http://localhost:8080/api'),
    },
    resolve: {
        alias: {
            '@app': path.resolve(__dirname, './src/app'),
            '@components': path.resolve(__dirname, './src/components'),
            '@features': path.resolve(__dirname, './src/features'),
            '@services': path.resolve(__dirname, './src/services'),
            '@hooks': path.resolve(__dirname, './src/hooks'),
            '@styles': path.resolve(__dirname, './src/styles'),
            '@tests': path.resolve(__dirname, './src/tests'),
            '@types': path.resolve(__dirname, './src/types'),
            '@pages': path.resolve(__dirname, './src/pages'),
        },
    },
    test: {
        globals: true,
        environment: 'jsdom',
        setupFiles: ['./src/tests/setupTests.ts'],
    },
})
