import { useState, useEffect } from 'react';
import jobTitlesService, { JobTitle } from '../services/jobTitles/jobTitlesService';

export const useJobTitles = () => {
    const [jobTitles, setJobTitles] = useState<JobTitle[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchJobTitles = async () => {
            try {
                setLoading(true);
                setError(null);
                const response = await jobTitlesService.getJobTitles({ isActive: true, pageSize: 100 });
                setJobTitles(response.items);
            } catch (err) {
                setError(err instanceof Error ? err.message : 'Erro ao carregar cargos');
            } finally {
                setLoading(false);
            }
        };

        fetchJobTitles();
    }, []);

    return { jobTitles, loading, error };
};

export const useAvailableJobTitles = () => {
    const [availableJobTitles, setAvailableJobTitles] = useState<JobTitle[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchAvailableJobTitles = async () => {
            try {
                setLoading(true);
                setError(null);
                const jobTitles = await jobTitlesService.getAvailableForCreation();
                setAvailableJobTitles(jobTitles);
            } catch (err) {
                setError(err instanceof Error ? err.message : 'Erro ao carregar cargos disponíveis');
            } finally {
                setLoading(false);
            }
        };

        fetchAvailableJobTitles();
    }, []);

    return { availableJobTitles, loading, error };
};

export const getJobTitleLevelName = (level: number): string => {
    switch (level) {
        case 1: return 'President';
        case 2: return 'Director';
        case 3: return 'Head';
        case 4: return 'Coordinator';
        case 5: return 'Employee';
        default: return 'Unknown';
    }
};

export const canCreateJobTitle = (userLevel: number, targetLevel: number): boolean => {
    // Usuários podem criar cargos de mesmo nível ou inferior
    return userLevel <= targetLevel;
};









