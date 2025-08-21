/** @type {import('jest').Config} */
const config = {
    preset: 'ts-jest',
    testEnvironment: 'jsdom',
    setupFilesAfterEnv: ['<rootDir>/src/tests/setupTests.ts'],
    moduleNameMapper: {
        '^@app/(.*)$': '<rootDir>/src/app/$1',
        '^@components/(.*)$': '<rootDir>/src/components/$1',
        '^@features/(.*)$': '<rootDir>/src/features/$1',
        '^@services/(.*)$': '<rootDir>/src/services/$1',
        '^@hooks/(.*)$': '<rootDir>/src/hooks/$1',
        '^@styles/(.*)$': '<rootDir>/src/styles/$1',
        '^@tests/(.*)$': '<rootDir>/src/tests/$1',
        '^@types/(.*)$': '<rootDir>/src/types/$1',
        '^@pages/(.*)$': '<rootDir>/src/pages/$1',
    },
    transform: {
        '^.+\\.(ts|tsx)$': ['ts-jest', {
            tsconfig: 'tsconfig.test.json',
            useESM: false
        }],
    },
    setupFiles: ['<rootDir>/src/tests/setupEnv.ts'],
    testMatch: [
        '<rootDir>/src/**/__tests__/**/*.{ts,tsx}',
        '<rootDir>/src/**/*.{test,spec}.{ts,tsx}',
    ],
    collectCoverageFrom: [
        'src/**/*.{ts,tsx}',
        '!src/**/*.d.ts',
        '!src/app/index.tsx',
        '!src/vite-env.d.ts',
    ],
    moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json'],
    // Configurações adicionais para melhor compatibilidade
    testEnvironmentOptions: {
        customExportConditions: ['node', 'node-addons'],
    },
    // Forçar CommonJS
    extensionsToTreatAsEsm: [],
    globals: {
        'ts-jest': {
            useESM: false
        }
    }
};

module.exports = config;
