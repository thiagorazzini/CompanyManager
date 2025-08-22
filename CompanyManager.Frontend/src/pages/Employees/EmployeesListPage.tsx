import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import Button from '@components/ui/Button';
import LoadingSpinner from '@components/LoadingSpinner';
import Table, { TableColumn } from '@components/ui/Table';
import UserHeader from '@components/layout/UserHeader';
import employeesService, { Employee } from '@services/employees/employeesService';
import toast from 'react-hot-toast';

const EmployeesListPage: React.FC = () => {

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
        if (!window.confirm(`Are you sure you want to delete ${employee.firstName} ${employee.lastName}?`)) {
            return;
        }

        try {
            setIsDeleting(employee.id);
            await employeesService.deleteEmployee(employee.id);
            toast.success('Employee deleted successfully!');
            await loadEmployees(); // Recarregar lista
        } catch (error) {
            toast.error('Error deleting employee');
        } finally {
            setIsDeleting(null);
        }
    };



    const columns: TableColumn<Employee>[] = [
        {
            key: 'name',
            header: 'Name',
            render: (_, employee) => `${employee.firstName} ${employee.lastName}`,
        },
        {
            key: 'email',
            header: 'Email',
        },
        {
            key: 'jobTitle',
            header: 'Job Title',
        },
        {
            key: 'departmentName',
            header: 'Department',
            render: (value) => value || 'Not assigned',
        },
        {
            key: 'createdAt',
            header: 'Creation Date',
            render: (value) => new Date(value).toLocaleDateString('en-US'),
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
            <UserHeader
                title="Employees"
                subtitle="Company employee management"
            />

            {/* Main Content */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="bg-white rounded-lg shadow p-6">
                    <div className="flex justify-between items-center mb-6">
                        <div className="flex items-center space-x-4">
                            <h2 className="text-lg font-medium text-gray-900">
                                Employee List
                            </h2>
                            <div className="flex space-x-2">
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => navigate('/dashboard')}
                                >
                                    🏠 Dashboard
                                </Button>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => navigate('/departments')}
                                >
                                    🏢 Departments
                                </Button>
                            </div>
                        </div>
                        <Button variant="primary" size="sm" onClick={handleAddEmployee}>
                            + Create New Employee
                        </Button>
                    </div>

                    <Table
                        data={employees}
                        columns={columns}
                        onEdit={handleEditEmployee}
                        onDelete={handleDeleteEmployee}
                        isLoading={isLoading}
                        emptyMessage="No employees found. Start by adding the first employee to the company."
                    />

                    {/* Loading overlay para exclusão */}
                    {isDeleting && (
                        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                            <div className="bg-white p-6 rounded-lg shadow-xl">
                                <div className="flex items-center space-x-3">
                                    <LoadingSpinner size="sm" />
                                    <span className="text-gray-700">Deleting...</span>
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


