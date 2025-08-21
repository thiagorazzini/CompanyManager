import { useState, useEffect } from 'react';
import departmentsService, { Department } from '@services/departments/departmentsService';

export const useDepartments = () => {
    const [departments, setDepartments] = useState<Department[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchDepartments = async () => {
            try {
                setLoading(true);
                setError(null);
                const data = await departmentsService.getDepartments();
                setDepartments(data);
            } catch (err) {
                setError(err instanceof Error ? err.message : 'Erro ao carregar departamentos');
            } finally {
                setLoading(false);
            }
        };

        fetchDepartments();
    }, []);

    return { departments, loading, error };
};

export const useAvailableDepartments = () => {
    const { departments, loading, error } = useDepartments();

    // Por enquanto, retorna todos os departamentos
    // No futuro, pode implementar lógica de filtragem baseada em permissões
    const availableDepartments = departments.map(dept => ({
        id: dept.id,
        name: dept.name,
        description: dept.description
    }));

    return {
        availableDepartments,
        loading,
        error
    };
};









