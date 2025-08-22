import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import Button from '@components/ui/Button';
import Input from '@components/ui/Input';
import LoadingSpinner from '@components/LoadingSpinner';
import UserHeader from '@components/layout/UserHeader';
import departmentsService, { UpdateDepartmentRequest, Department } from '@services/departments/departmentsService';
import toast from 'react-hot-toast';

const DepartmentsEditPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();

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
                description: data.description || '',
            });
        } catch (error) {
            toast.error('Error loading department');
            navigate('/departments');
        } finally {
            setIsInitialLoading(false);
        }
    };

    const validateForm = (): boolean => {
        const newErrors: Partial<UpdateDepartmentRequest> = {};

        if (!form.name.trim()) {
            newErrors.name = 'Name is required';
        }

        if (!form.description?.trim()) {
            newErrors.description = 'Description is required';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof UpdateDepartmentRequest, value: string) => {
        setForm(prev => ({ ...prev, [field]: value }));
        // Clear field error when user starts typing
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
            toast.success('Department updated successfully!');
            navigate('/departments');
        } catch (error) {
            toast.error('Error updating department');
        } finally {
            setIsLoading(false);
        }
    };

    const handleCancel = () => {
        navigate('/departments');
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
                        Department not found
                    </h2>
                    <Button variant="primary" onClick={() => navigate('/departments')}>
                        Back to Departments
                    </Button>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50">
            <UserHeader
                title="Edit Department"
                subtitle="Update department information"
            />

            {/* Main Content */}
            <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="bg-white rounded-lg shadow p-6">
                    <form onSubmit={handleSubmit} className="space-y-6">
                        {/* Nome */}
                        <div>
                            <Input
                                label="Department Name"
                                type="text"
                                value={form.name}
                                onChange={(value) => handleInputChange('name', value)}
                                placeholder="Ex: Human Resources"
                                error={errors.name}
                                disabled={isLoading}
                                required={true}
                            />
                        </div>

                        {/* Descrição */}
                        <div>
                            <Input
                                label="Description"
                                type="text"
                                value={form.description || ''}
                                onChange={(value) => handleInputChange('description', value)}
                                placeholder="Describe the department's responsibilities and functions"
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
                                            Saving...
                                        </>
                                    ) : (
                                        'Save Changes'
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


