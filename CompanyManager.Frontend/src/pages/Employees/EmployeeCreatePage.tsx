import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';
import Button from '@components/ui/Button';
import { Input, Select, Form, FormRow, FormActions } from '@components/ui/Form';

import UserHeader from '@components/layout/UserHeader';
import employeesService, { CreateEmployeeRequest } from '@services/employees/employeesService';
import { useAvailableJobTitles, getJobTitleLevelName, canCreateJobTitle } from '@hooks/useJobTitles';
import { useAvailableDepartments } from '@hooks/useDepartments';
import toast from 'react-hot-toast';

const EmployeeCreatePage: React.FC = () => {
    const { userProfile } = useAuth();
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
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
    const [errors, setErrors] = useState<Partial<CreateEmployeeRequest>>({});

    // Get current user's hierarchical level dynamically
    const currentUserLevel = userProfile?.jobTitle?.hierarchyLevel || 1;

    const validateForm = (): boolean => {
        const newErrors: Partial<CreateEmployeeRequest> = {};

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
        if (!form.dateOfBirth) {
            newErrors.dateOfBirth = 'Data de nascimento é obrigatória';
        }
        if (!form.jobTitleId) {
            newErrors.jobTitleId = 'Cargo é obrigatório';
        }
        if (!form.departmentId) {
            newErrors.departmentId = 'Departamento é obrigatório';
        }
        if (!form.password.trim()) {
            newErrors.password = 'Senha é obrigatória';
        }
        // roleLevel removed - level is determined by JobTitle.HierarchyLevel

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof CreateEmployeeRequest, value: string) => {
        setForm(prev => ({ ...prev, [field]: value }));

        // Clear error when user starts typing
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

            // Prepare data for submission
            const submitData: CreateEmployeeRequest = {
                ...form,
                phoneNumbers: form.phoneNumbers.filter(phone => phone.trim() !== '')
            };

            await employeesService.createEmployee(submitData);
            toast.success('Funcionário criado com sucesso!');
            navigate('/employees');
        } catch (error) {
            toast.error('Error creating employee. Please try again.');
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

    return (
        <div className="min-h-screen bg-gray-50">
            <UserHeader
                title="Create Employee"
                subtitle="Add new employee to the company"
            />

            {/* Main Content */}
            <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="bg-white rounded-lg shadow p-6">
                    <Form onSubmit={handleSubmit}>
                        <FormRow>
                            <Input
                                label="First Name"
                                name="firstName"
                                value={form.firstName}
                                onChange={(value) => handleInputChange('firstName', value)}
                                error={errors.firstName}
                                required
                                placeholder="Enter first name"
                            />
                            <Input
                                label="Last Name"
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
                                label="Date of Birth"
                                name="dateOfBirth"
                                type="date"
                                value={form.dateOfBirth}
                                onChange={(value) => handleInputChange('dateOfBirth', value)}
                                error={errors.dateOfBirth}
                                required
                            />
                            <Input
                                label="Phone"
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
                        </FormRow>

                        <FormRow>
                            <Select
                                label="Job Title"
                                name="jobTitle"
                                value={form.jobTitleId}
                                onChange={(value) => handleInputChange('jobTitleId', value)}
                                options={availableJobTitleOptions}
                                error={errors.jobTitleId}
                                required
                                placeholder={jobTitlesLoading ? "Loading job titles..." : "Select job title"}
                            />
                            <Select
                                label="Department"
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
                        </FormRow>

                        <FormRow>
                            <Input
                                label="Password"
                                name="password"
                                type="password"
                                value={form.password}
                                onChange={(value) => handleInputChange('password', value)}
                                error={errors.password}
                                required
                                placeholder="Enter password"
                            />
                        </FormRow>

                        {/* Informação sobre hierarquia */}
                        <div className="bg-blue-50 border border-blue-200 rounded-md p-4 mb-6">
                            <div className="flex">
                                <div className="flex-shrink-0">
                                    <svg className="h-5 w-5 text-blue-400" viewBox="0 0 20 20" fill="currentColor">
                                        <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                                    </svg>
                                </div>
                                <div className="ml-3">
                                    <h3 className="text-sm font-medium text-blue-800">
                                        Job Title Hierarchy
                                    </h3>
                                    <div className="mt-2 text-sm text-blue-700">
                                        <p>You can create employees with job titles at the same level or below yours.</p>
                                        <p className="mt-1">
                                            <strong>User:</strong> {userProfile?.firstName && userProfile?.lastName
                                                ? `${userProfile.firstName} ${userProfile.lastName}`
                                                : 'Name not available'}
                                        </p>
                                        <p className="mt-1">
                                            <strong>Email:</strong> {userProfile?.email || 'Email not available'}
                                        </p>
                                        <p className="mt-1">
                                            <strong>Current Level:</strong> {userProfile?.jobTitle?.name || getJobTitleLevelName(currentUserLevel)}
                                        </p>
                                        <p className="mt-1 text-xs text-blue-600">
                                            💡 The hierarchical level is automatically set by the selected job title
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>

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
                                    {isLoading ? 'Creating...' : 'Create Employee'}
                                </Button>
                            </div>
                        </FormActions>
                    </Form>
                </div>
            </div>
        </div>
    );
};

export default EmployeeCreatePage;


