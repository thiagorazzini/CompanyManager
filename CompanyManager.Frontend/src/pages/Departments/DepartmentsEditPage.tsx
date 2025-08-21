import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';
import Button from '@components/ui/Button';
import Input from '@components/ui/Input';
import LoadingSpinner from '@components/LoadingSpinner';
import departmentsService, { UpdateDepartmentRequest, Department } from '@services/departments/departmentsService';
import toast from 'react-hot-toast';

const DepartmentsEditPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [isInitialLoading, setIsInitialLoading] = useState(true);
    const [department, setDepartment] = useState<Department | null>(null);
    const [form, setForm] = useState<UpdateDepartmentRequest>({
        name: '',
        description: '',
    });
    const [errors, setErrors] = useState<Partial<UpdateDepartmentRequest>>({});

    useEffect(() => {
        if (id) {
            loadDepartment();
        }
    }, [id]);

    const loadDepartment = async () => {
        try {
            setIsInitialLoading(true);
            const data = await departmentsService.getDepartmentById(id!);
            setDepartment(data);
            setForm({
                name: data.name,
                description: data.description,
            });
        } catch (error) {
            toast.error('Erro ao carregar departamento');
            navigate('/departments');
        } finally {
            setIsInitialLoading(false);
        }
    };

    const validateForm = (): boolean => {
        const newErrors: Partial<UpdateDepartmentRequest> = {};

        if (!form.name.trim()) {
            newErrors.name = 'Nome é obrigatório';
        }

        if (!form.description.trim()) {
            newErrors.description = 'Descrição é obrigatória';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof UpdateDepartmentRequest, value: string) => {
        setForm(prev => ({ ...prev, [field]: value }));
        // Limpar erro do campo quando o usuário começar a digitar
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm() || !id) {
            return;
        }

        try {
            setIsLoading(true);
            await departmentsService.updateDepartment(id, form);
            toast.success('Departamento atualizado com sucesso!');
            navigate('/departments');
        } catch (error) {
            toast.error('Erro ao atualizar departamento');
        } finally {
            setIsLoading(false);
        }
    };

    const handleCancel = () => {
        navigate('/departments');
    };

    const handleLogout = () => {
        logout();
    };

    if (isInitialLoading) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <LoadingSpinner size="lg" />
            </div>
        );
    }

    if (!department) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <h2 className="text-xl font-semibold text-gray-900 mb-2">
                        Departamento não encontrado
                    </h2>
                    <Button variant="primary" onClick={() => navigate('/departments')}>
                        Voltar para Departamentos
                    </Button>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Header */}
            <div className="bg-white shadow-sm border-b border-gray-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="flex justify-between items-center py-4">
                        <div>
                            <h1 className="text-2xl font-bold text-gray-900">Editar Departamento</h1>
                            <p className="text-sm text-gray-600">
                                Atualizar informações do departamento
                            </p>
                        </div>
                        <div className="flex items-center space-x-4">
                            <div className="text-right">
                                <p className="text-sm font-medium text-gray-900">
                                    {user?.username || 'Usuário'}
                                </p>
                                <p className="text-xs text-gray-500">
                                    {user?.email || 'email@exemplo.com'}
                                </p>
                            </div>
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={handleLogout}
                            >
                                Sair
                            </Button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Main Content */}
            <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="bg-white rounded-lg shadow p-6">
                    <form onSubmit={handleSubmit} className="space-y-6">
                        {/* Nome */}
                        <div>
                            <Input
                                label="Nome do Departamento"
                                type="text"
                                value={form.name}
                                onChange={(value) => handleInputChange('name', value)}
                                placeholder="Ex: Recursos Humanos"
                                error={errors.name}
                                disabled={isLoading}
                                required={true}
                            />
                        </div>

                        {/* Descrição */}
                        <div>
                            <Input
                                label="Descrição"
                                type="text"
                                value={form.description}
                                onChange={(value) => handleInputChange('description', value)}
                                placeholder="Descreva as responsabilidades e funções do departamento"
                                error={errors.description}
                                disabled={isLoading}
                                required={true}
                            />
                        </div>

                        {/* Botões */}
                        <div className="flex justify-between pt-4 border-t border-gray-200">
                            {/* Botões da esquerda */}
                            <div className="flex space-x-3">
                                <Button
                                    type="button"
                                    variant="outline"
                                    onClick={() => navigate('/departments')}
                                    disabled={isLoading}
                                >
                                    ← Voltar para Departamentos
                                </Button>
                                <Button
                                    type="button"
                                    variant="outline"
                                    onClick={() => navigate('/dashboard')}
                                    disabled={isLoading}
                                >
                                    🏠 Dashboard
                                </Button>
                            </div>

                            {/* Botões da direita */}
                            <div className="flex space-x-3">
                                <Button
                                    type="button"
                                    variant="outline"
                                    onClick={handleCancel}
                                    disabled={isLoading}
                                >
                                    Cancelar
                                </Button>
                                <Button
                                    type="submit"
                                    variant="primary"
                                    disabled={isLoading}
                                >
                                    {isLoading ? (
                                        <>
                                            <LoadingSpinner size="sm" className="mr-2" />
                                            Salvando...
                                        </>
                                    ) : (
                                        'Salvar Alterações'
                                    )}
                                </Button>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default DepartmentsEditPage;


