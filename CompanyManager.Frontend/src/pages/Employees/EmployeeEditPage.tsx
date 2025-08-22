import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import Button from '@components/ui/Button';
import { Input, Select, Form, FormRow, FormActions } from '@components/ui/Form';
import LoadingSpinner from '@components/LoadingSpinner';
import UserHeader from '@components/layout/UserHeader';
import employeesService, { UpdateEmployeeRequest, EmployeeDetail } from '@services/employees/employeesService';
import { useAvailableJobTitles, getJobTitleLevelName, canCreateJobTitle } from '@hooks/useJobTitles';
import { useAvailableDepartments } from '@hooks/useDepartments';
import toast from 'react-hot-toast';

const EmployeeEditPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();

    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [isInitialLoading, setIsInitialLoading] = useState(true);
    const [employee, setEmployee] = useState<EmployeeDetail | null>(null);
    const { availableJobTitles, loading: jobTitlesLoading, error: jobTitlesError } = useAvailableJobTitles();
    const { availableDepartments, loading: departmentsLoading, error: departmentsError } = useAvailableDepartments();
    const [form, setForm] = useState({
        firstName: '',
        lastName: '',
        email: '',
        documentNumber: '',
        dateOfBirth: '',
        phoneNumbers: [''],
        jobTitleId: '',
        departmentId: '',
        password: ''
    });
    const [errors, setErrors] = useState<Partial<UpdateEmployeeRequest>>({});

    // Simulate current user's hierarchical level (temporary)
    const currentUserLevel = 1; // President - can create all levels

    useEffect(() => {
        if (id) {
            loadEmployee();
        }
    }, [id]);

    const loadEmployee = async () => {
        if (!id) return;

        try {
            const data = await employeesService.getEmployeeById(id);
            setEmployee(data);
            setForm({
                firstName: data.firstName,
                lastName: data.lastName,
                email: data.email,
                documentNumber: data.documentNumber,
                dateOfBirth: data.dateOfBirth || '',
                phoneNumbers: data.phoneNumbers,
                jobTitleId: data.jobTitleId,
                departmentId: data.departmentId,
                password: '',
            });
        } catch (error) {
            toast.error('Erro ao carregar funcionário');
        } finally {
            setIsInitialLoading(false);
        }
    };



    const validateForm = (): boolean => {
        const newErrors: Partial<UpdateEmployeeRequest> = {};

        if (!form.firstName.trim()) {
            newErrors.firstName = 'Nome é obrigatório';
        }
        if (!form.lastName.trim()) {
            newErrors.lastName = 'Sobrenome é obrigatório';
        }
        if (!form.email.trim()) {
            newErrors.email = 'Email é obrigatório';
        } else if (!/\S+@\S+\.\S+/.test(form.email)) {
            newErrors.email = 'Email inválido';
        }
        if (!form.documentNumber.trim()) {
            newErrors.documentNumber = 'CPF é obrigatório';
        }
        if (!form.phoneNumbers.length || !form.phoneNumbers[0].trim()) {
            (newErrors as any).phoneNumbers = 'Telefone é obrigatório';
        }
        if (!form.jobTitleId) {
            newErrors.jobTitleId = 'Cargo é obrigatório';
        }
        if (!form.departmentId) {
            newErrors.departmentId = 'Departamento é obrigatório';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof UpdateEmployeeRequest, value: string) => {
        setForm(prev => ({ ...prev, [field]: value }));

        // Clear error when user starts typing
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const handlePasswordChange = (value: string) => {
        setForm(prev => ({ ...prev, password: value }));

        // Clear error when user starts typing
        if (errors.password) {
            setErrors(prev => ({ ...prev, password: undefined }));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm() || !id) {
            return;
        }

        try {
            setIsLoading(true);

            // Garantir que o managerId seja sempre enviado e filtrar campos vazios
            const updateData = {
                ...form,
                // Incluir password apenas se foi preenchido
                ...(form.password && { password: form.password })
            };

            await employeesService.updateEmployee(id, updateData);
            toast.success('Funcionário atualizado com sucesso!');
            navigate('/employees');
        } catch (error) {
            toast.error('Erro ao atualizar funcionário');
        } finally {
            setIsLoading(false);
        }
    };

    const handleCancel = () => {
        navigate('/employees');
    };



    // Filter job titles based on user hierarchy
    const availableJobTitleOptions = availableJobTitles
        .filter(jobTitle => canCreateJobTitle(currentUserLevel, jobTitle.hierarchyLevel))
        .map(jobTitle => ({
            value: jobTitle.id,
            label: `${jobTitle.name} (${getJobTitleLevelName(jobTitle.hierarchyLevel)})`,
            disabled: false
        }));

    // Show error if unable to load job titles
    if (jobTitlesError) {
        toast.error(`Error loading job titles: ${jobTitlesError}`);
    }

    // Show error if unable to load departments
    if (departmentsError) {
        toast.error(`Error loading departments: ${departmentsError}`);
    }

    if (isInitialLoading) {
        return (
            <div className="min-h-screen bg-gray-50">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                    <LoadingSpinner size="lg" />
                </div>
            </div>
        );
    }

    if (!employee) {
        return (
            <div className="min-h-screen bg-gray-50">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                    <div className="bg-white rounded-lg shadow p-6 text-center">
                        <div className="text-gray-400 mb-4">
                            <svg className="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                            </svg>
                        </div>
                        <h3 className="text-lg font-medium text-gray-900 mb-2">
                            Funcionário não encontrado
                        </h3>
                        <p className="text-gray-500 mb-4">
                            The employee you are trying to edit was not found.
                        </p>
                        <Button variant="primary" onClick={handleCancel}>
                            Back to Employees
                        </Button>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50">
            <UserHeader
                title="Edit Employee"
                subtitle="Modify employee data"
            />

            {/* Main Content */}
            <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="bg-white rounded-lg shadow p-6">
                    <Form onSubmit={handleSubmit}>
                        <FormRow>
                            <Input
                                label="Nome"
                                name="firstName"
                                value={form.firstName}
                                onChange={(value) => handleInputChange('firstName', value)}
                                error={errors.firstName}
                                required
                                placeholder="Enter first name"
                            />
                            <Input
                                label="Sobrenome"
                                name="lastName"
                                value={form.lastName}
                                onChange={(value) => handleInputChange('lastName', value)}
                                error={errors.lastName}
                                required
                                placeholder="Enter last name"
                            />
                        </FormRow>

                        <FormRow>
                            <Input
                                label="Email"
                                name="email"
                                type="email"
                                value={form.email}
                                onChange={(value) => handleInputChange('email', value)}
                                error={errors.email}
                                required
                                placeholder="Enter email"
                            />
                            <Input
                                label="CPF"
                                name="documentNumber"
                                type="text"
                                value={form.documentNumber}
                                onChange={(value) => handleInputChange('documentNumber', value)}
                                error={errors.documentNumber}
                                required
                                placeholder="000.000.000-00"
                            />
                        </FormRow>

                        <FormRow>
                            <Input
                                label="Telefone"
                                name="phoneNumbers"
                                type="tel"
                                value={form.phoneNumbers[0] || ''}
                                onChange={(value) => {
                                    const newPhoneNumbers = [...form.phoneNumbers];
                                    newPhoneNumbers[0] = value;
                                    setForm(prev => ({ ...prev, phoneNumbers: newPhoneNumbers }));
                                }}
                                error={typeof errors.phoneNumbers === 'string' ? errors.phoneNumbers : undefined}
                                required
                                placeholder="(11) 99999-9999"
                            />
                            <Select
                                label="Cargo"
                                name="jobTitle"
                                value={form.jobTitleId}
                                onChange={(value) => handleInputChange('jobTitleId', value)}
                                options={availableJobTitleOptions}
                                error={errors.jobTitleId}
                                required
                                placeholder={jobTitlesLoading ? "Loading job titles..." : "Select job title"}
                            />
                        </FormRow>

                        <FormRow>
                            <Select
                                label="Departamento"
                                name="departmentId"
                                value={form.departmentId}
                                onChange={(value) => handleInputChange('departmentId', value)}
                                options={availableDepartments.map(dept => ({
                                    value: dept.id,
                                    label: dept.name
                                }))}
                                error={errors.departmentId}
                                required
                                placeholder={departmentsLoading ? "Loading departments..." : "Select department"}
                            />
                            <Input
                                label="Nova Senha (opcional)"
                                name="password"
                                type="password"
                                value={form.password || ''}
                                onChange={handlePasswordChange}
                                error={errors.password}
                                placeholder="Deixe em branco para manter a senha atual"
                            />
                        </FormRow>

                        <FormActions justify="between">
                            {/* Botões da esquerda */}
                            <div className="flex space-x-3">
                                <Button
                                    type="button"
                                    variant="outline"
                                    onClick={() => navigate('/employees')}
                                    disabled={isLoading}
                                >
                                    ← Back to Employees
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
                                    loading={isLoading}
                                    disabled={isLoading}
                                >
                                    {isLoading ? 'Saving...' : 'Save Changes'}
                                </Button>
                            </div>
                        </FormActions>
                    </Form>
                </div>
            </div>
        </div>
    );
};

export default EmployeeEditPage;


