import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';
import Button from '@components/ui/Button';
import LoadingSpinner from '@components/LoadingSpinner';
import Table, { TableColumn } from '@components/ui/Table';
import employeesService, { Employee } from '@services/employees/employeesService';
import toast from 'react-hot-toast';

const EmployeesListPage: React.FC = () => {
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDeleting, setIsDeleting] = useState<string | null>(null);

    useEffect(() => {
        loadEmployees();
    }, []);

    const loadEmployees = async () => {
        try {
            setIsLoading(true);
            const data = await employeesService.getEmployees();
            setEmployees(data);
        } catch (error) {
            toast.error('Erro ao carregar funcionários');
        } finally {
            setIsLoading(false);
        }
    };

    const handleAddEmployee = () => {
        navigate('/employees/create');
    };

    const handleEditEmployee = (employee: Employee) => {
        navigate(`/employees/edit/${employee.id}`);
    };

    const handleDeleteEmployee = async (employee: Employee) => {
        if (!window.confirm(`Tem certeza que deseja deletar ${employee.firstName} ${employee.lastName}?`)) {
            return;
        }

        try {
            setIsDeleting(employee.id);
            await employeesService.deleteEmployee(employee.id);
            toast.success('Funcionário deletado com sucesso!');
            await loadEmployees(); // Recarregar lista
        } catch (error) {
            toast.error('Erro ao deletar funcionário');
        } finally {
            setIsDeleting(null);
        }
    };

    const handleLogout = () => {
        logout();
    };

    const columns: TableColumn<Employee>[] = [
        {
            key: 'name',
            header: 'Nome',
            render: (_, employee) => `${employee.firstName} ${employee.lastName}`,
        },
        {
            key: 'email',
            header: 'Email',
        },
        {
            key: 'jobTitle',
            header: 'Cargo',
        },
        {
            key: 'departmentName',
            header: 'Departamento',
            render: (value) => value || 'Não atribuído',
        },
        {
            key: 'createdAt',
            header: 'Data de Criação',
            render: (value) => new Date(value).toLocaleDateString('pt-BR'),
        },
    ];

    if (isLoading) {
        return (
            <div className="min-h-screen bg-gray-50">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                    <LoadingSpinner size="lg" />
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
                            <h1 className="text-2xl font-bold text-gray-900">Funcionários</h1>
                            <p className="text-sm text-gray-600">
                                Gerenciamento de funcionários da empresa
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
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="bg-white rounded-lg shadow p-6">
                    <div className="flex justify-between items-center mb-6">
                        <h2 className="text-lg font-medium text-gray-900">
                            Lista de Funcionários
                        </h2>
                        <Button variant="primary" size="sm" onClick={handleAddEmployee}>
                            + Criar Novo Funcionário
                        </Button>
                    </div>

                    <Table
                        data={employees}
                        columns={columns}
                        onEdit={handleEditEmployee}
                        onDelete={handleDeleteEmployee}
                        isLoading={isLoading}
                        emptyMessage="Nenhum funcionário encontrado. Comece adicionando o primeiro funcionário à empresa."
                    />

                    {/* Loading overlay para exclusão */}
                    {isDeleting && (
                        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                            <div className="bg-white p-6 rounded-lg shadow-xl">
                                <div className="flex items-center space-x-3">
                                    <LoadingSpinner size="sm" />
                                    <span className="text-gray-700">Deletando...</span>
                                </div>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default EmployeesListPage;


