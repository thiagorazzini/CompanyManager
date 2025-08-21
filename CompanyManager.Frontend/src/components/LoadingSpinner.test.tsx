import React from 'react';
import { render } from '@testing-library/react';
import LoadingSpinner from './LoadingSpinner';

describe('LoadingSpinner Component', () => {
    it('deve renderizar com tamanho padrão (md)', () => {
        const { container } = render(<LoadingSpinner />);
        const spinner = container.querySelector('.h-8.w-8');
        expect(spinner).toBeInTheDocument();
    });

    it('deve renderizar com tamanho pequeno (sm)', () => {
        const { container } = render(<LoadingSpinner size="sm" />);
        const spinner = container.querySelector('.h-4.w-4');
        expect(spinner).toBeInTheDocument();
    });

    it('deve renderizar com tamanho grande (lg)', () => {
        const { container } = render(<LoadingSpinner size="lg" />);
        const spinner = container.querySelector('.h-12.w-12');
        expect(spinner).toBeInTheDocument();
    });

    it('deve aplicar classes customizadas', () => {
        const { container } = render(<LoadingSpinner className="custom-class" />);
        const spinner = container.querySelector('.custom-class');
        expect(spinner).toBeInTheDocument();
    });

    it('deve ter a classe animate-spin', () => {
        const { container } = render(<LoadingSpinner />);
        const spinner = container.querySelector('.animate-spin');
        expect(spinner).toBeInTheDocument();
    });
});
