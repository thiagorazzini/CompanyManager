import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';
import Button from '@components/ui/Button';
import { Input, Select, Form, FormRow, FormActions } from '@components/ui/Form';
import LoadingSpinner from '@components/LoadingSpinner';
import employeesService, { CreateEmployeeRequest } from '@services/employees/employeesService';
import { useAvailableJobTitles, getJobTitleLevelName, canCreateJobTitle } from '@hooks/useJobTitles';
import { useAvailableDepartments } from '@hooks/useDepartments';
import toast from 'react-hot-toast';

const EmployeeCreatePage: React.FC = () => {
    const { user, logout } = useAuth();
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

    // Simular nível hierárquico do usuário atual (por enquanto)
    // TODO: Implementar lógica real baseada no usuário logado
    const currentUserLevel = 1; // President - pode criar todos os níveis

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
        // roleLevel removido - o nível é determinado pelo JobTitle.HierarchyLevel

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof CreateEmployeeRequest, value: string) => {
        setForm(prev => ({ ...prev, [field]: value }));

        // Limpar erro quando o usuário começar a digitar
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

            // Preparar dados para envio
            const submitData: CreateEmployeeRequest = {
                ...form,
                phoneNumbers: form.phoneNumbers.filter(phone => phone.trim() !== '')
            };

            await employeesService.createEmployee(submitData);
            toast.success('Funcionário criado com sucesso!');
            navigate('/employees');
        } catch (error) {
            console.error('Erro ao criar funcionário:', error);
            toast.error('Erro ao criar funcionário. Tente novamente.');
        } finally {
            setIsLoading(false);
        }
    };

    const handleCancel = () => {
        navigate('/employees');
    };

    const handleLogout = () => {
        logout();
    };

    // Filtrar cargos baseado na hierarquia do usuário
    const availableJobTitleOptions = availableJobTitles
        .filter(jobTitle => canCreateJobTitle(currentUserLevel, jobTitle.hierarchyLevel))
        .map(jobTitle => ({
            value: jobTitle.id,
            label: `${jobTitle.name} (${getJobTitleLevelName(jobTitle.hierarchyLevel)})`,
            disabled: false
        }));

    // Mostrar erro se não conseguir carregar cargos
    if (jobTitlesError) {
        toast.error(`Erro ao carregar cargos: ${jobTitlesError}`);
    }

    // Mostrar erro se não conseguir carregar departamentos
    if (departmentsError) {
        toast.error(`Erro ao carregar departamentos: ${departmentsError}`);
    }

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Header */}
            <div className="bg-white shadow-sm border-b border-gray-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="flex justify-between items-center py-4">
                        <div>
                            <h1 className="text-2xl font-bold text-gray-900">Criar Funcionário</h1>
                            <p className="text-sm text-gray-600">
                                Adicionar novo funcionário à empresa
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
                            <Button variant="outline" size="sm" onClick={handleLogout}>
                                Sair
                            </Button>
                        </div>
                    </div>
                </div>
            </div>

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
                                placeholder="Digite o nome"
                            />
                            <Input
                                label="Sobrenome"
                                name="lastName"
                                value={form.lastName}
                                onChange={(value) => handleInputChange('lastName', value)}
                                error={errors.lastName}
                                required
                                placeholder="Digite o sobrenome"
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
                                placeholder="Digite o email"
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
                                label="Data de Nascimento"
                                name="dateOfBirth"
                                type="date"
                                value={form.dateOfBirth}
                                onChange={(value) => handleInputChange('dateOfBirth', value)}
                                error={errors.dateOfBirth}
                                required
                            />
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
                        </FormRow>

                        <FormRow>
                            <Select
                                label="Cargo"
                                name="jobTitle"
                                value={form.jobTitleId}
                                onChange={(value) => handleInputChange('jobTitleId', value)}
                                options={availableJobTitleOptions}
                                error={errors.jobTitleId}
                                required
                                placeholder={jobTitlesLoading ? "Carregando cargos..." : "Selecione o cargo"}
                            />
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
                                placeholder={departmentsLoading ? "Carregando departamentos..." : "Selecione o departamento"}
                            />
                        </FormRow>

                        <FormRow>
                            <Input
                                label="Senha"
                                name="password"
                                type="password"
                                value={form.password}
                                onChange={(value) => handleInputChange('password', value)}
                                error={errors.password}
                                required
                                placeholder="Digite a senha"
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
                                        Hierarquia de Cargos
                                    </h3>
                                    <div className="mt-2 text-sm text-blue-700">
                                        <p>Você pode criar funcionários com cargos de mesmo nível ou inferior ao seu.</p>
                                        <p className="mt-1">Nível atual: <strong>{getJobTitleLevelName(currentUserLevel)}</strong></p>
                                        <p className="mt-1 text-xs text-blue-600">
                                            💡 O nível hierárquico é automaticamente definido pelo cargo selecionado
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
                                    ← Voltar para Funcionários
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
                                    loading={isLoading}
                                    disabled={isLoading}
                                >
                                    {isLoading ? 'Criando...' : 'Criar Funcionário'}
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


