import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import Button from '@components/ui/Button';
import Input from '@components/ui/Input';
import LoadingSpinner from '@components/LoadingSpinner';
import UserHeader from '@components/layout/UserHeader';
import departmentsService, { CreateDepartmentRequest } from '@services/departments/departmentsService';
import toast from 'react-hot-toast';

const DepartmentsCreatePage: React.FC = () => {

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

        if (!form.description?.trim()) {
            newErrors.description = 'Descrição é obrigatória';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof CreateDepartmentRequest, value: string) => {
        setForm(prev => ({ ...prev, [field]: value }));
        // Clear field error when user starts typing
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
            toast.error('Error creating department');
        } finally {
            setIsLoading(false);
        }
    };

    const handleCancel = () => {
        navigate('/departments');
    };



    return (
        <div className="min-h-screen bg-gray-50">
            <UserHeader
                title="Create Department"
                subtitle="Add new department to the company"
            />

            {/* Main Content */}
            <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="bg-white rounded-lg shadow p-6">
                    <form onSubmit={handleSubmit} className="space-y-6">
                        {/* Nome */}
                        <div>
                            <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-2">
                                Department Name *
                            </label>
                            <Input
                                id="name"
                                type="text"
                                value={form.name}
                                onChange={(value) => handleInputChange('name', value)}
                                placeholder="Ex: Human Resources"
                                error={errors.name}
                                disabled={isLoading}
                            />
                        </div>

                        {/* Descrição */}
                        <div>
                            <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
                                Description *
                            </label>
                            <textarea
                                id="description"
                                rows={4}
                                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${errors.description ? 'border-red-300' : 'border-gray-300'
                                    } ${isLoading ? 'bg-gray-100' : 'bg-white'}`}
                                value={form.description}
                                onChange={(e) => handleInputChange('description', e.target.value)}
                                placeholder="Describe the department's responsibilities and functions"
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
                                    ← Back to Departments
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
                                    Cancel
                                </Button>
                                <Button
                                    type="submit"
                                    variant="primary"
                                    disabled={isLoading}
                                >
                                    {isLoading ? (
                                        <>
                                            <LoadingSpinner size="sm" className="mr-2" />
                                            Creating...
                                        </>
                                    ) : (
                                        'Create Department'
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


