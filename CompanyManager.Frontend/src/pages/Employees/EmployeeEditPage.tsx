import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';
import Button from '@components/ui/Button';
import { Input, Select, Form, FormRow, FormActions } from '@components/ui/Form';
import LoadingSpinner from '@components/LoadingSpinner';
import employeesService, { UpdateEmployeeRequest, EmployeeDetail } from '@services/employees/employeesService';
import { useAvailableJobTitles, getJobTitleLevelName, canCreateJobTitle } from '@hooks/useJobTitles';
import { useAvailableDepartments } from '@hooks/useDepartments';
import toast from 'react-hot-toast';

const EmployeeEditPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const { user, logout } = useAuth();
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

    // Simular nível hierárquico do usuário atual (por enquanto)
    const currentUserLevel = 1; // President - pode criar todos os níveis

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

        // Limpar erro quando o usuário começar a digitar
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const handlePasswordChange = (value: string) => {
        setForm(prev => ({ ...prev, password: value }));

        // Limpar erro quando o usuário começar a digitar
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
                            O funcionário que você está tentando editar não foi encontrado.
                        </p>
                        <Button variant="primary" onClick={handleCancel}>
                            Voltar para Funcionários
                        </Button>
                    </div>
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
                            <h1 className="text-2xl font-bold text-gray-900">Editar Funcionário</h1>
                            <p className="text-sm text-gray-600">
                                Modificar dados do funcionário
                            </p>
                        </div>
                        <div className="flex items-center space-x-4">
                            <div className="text-right">
                                <p className="text-sm font-medium text-gray-900">
                                    {user?.firstName || 'Usuário'}
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
                                placeholder={jobTitlesLoading ? "Carregando cargos..." : "Selecione o cargo"}
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
                                placeholder={departmentsLoading ? "Carregando departamentos..." : "Selecione o departamento"}
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

                        <FormActions>
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
                                {isLoading ? 'Salvando...' : 'Salvar Alterações'}
                            </Button>
                        </FormActions>
                    </Form>
                </div>
            </div>
        </div>
    );
};

export default EmployeeEditPage;


