import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';
import Button from '@components/ui/Button';
import Input from '@components/ui/Input';
import LoadingSpinner from '@components/LoadingSpinner';
import departmentsService, { CreateDepartmentRequest } from '@services/departments/departmentsService';
import toast from 'react-hot-toast';

const DepartmentsCreatePage: React.FC = () => {
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [form, setForm] = useState<CreateDepartmentRequest>({
        name: '',
        description: '',
    });
    const [errors, setErrors] = useState<Partial<CreateDepartmentRequest>>({});

    const validateForm = (): boolean => {
        const newErrors: Partial<CreateDepartmentRequest> = {};

        if (!form.name.trim()) {
            newErrors.name = 'Nome é obrigatório';
        }

        if (!form.description.trim()) {
            newErrors.description = 'Descrição é obrigatória';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof CreateDepartmentRequest, value: string) => {
        setForm(prev => ({ ...prev, [field]: value }));
        // Limpar erro do campo quando o usuário começar a digitar
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        try {
            setIsLoading(true);
            await departmentsService.createDepartment(form);
            toast.success('Departamento criado com sucesso!');
            navigate('/departments');
        } catch (error) {
            toast.error('Erro ao criar departamento');
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

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Header */}
            <div className="bg-white shadow-sm border-b border-gray-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="flex justify-between items-center py-4">
                        <div>
                            <h1 className="text-2xl font-bold text-gray-900">Criar Departamento</h1>
                            <p className="text-sm text-gray-600">
                                Adicionar novo departamento à empresa
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
                            <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-2">
                                Nome do Departamento *
                            </label>
                            <Input
                                id="name"
                                type="text"
                                value={form.name}
                                onChange={(value) => handleInputChange('name', value)}
                                placeholder="Ex: Recursos Humanos"
                                error={errors.name}
                                disabled={isLoading}
                            />
                        </div>

                        {/* Descrição */}
                        <div>
                            <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
                                Descrição *
                            </label>
                            <textarea
                                id="description"
                                rows={4}
                                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${errors.description ? 'border-red-300' : 'border-gray-300'
                                    } ${isLoading ? 'bg-gray-100' : 'bg-white'}`}
                                value={form.description}
                                onChange={(e) => handleInputChange('description', e.target.value)}
                                placeholder="Descreva as responsabilidades e funções do departamento"
                                disabled={isLoading}
                            />
                            {errors.description && (
                                <p className="mt-1 text-sm text-red-600">{errors.description}</p>
                            )}
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
                                            Criando...
                                        </>
                                    ) : (
                                        'Criar Departamento'
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

export default DepartmentsCreatePage;


