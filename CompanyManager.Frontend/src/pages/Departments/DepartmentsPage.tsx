import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import Button from '@components/ui/Button';
import LoadingSpinner from '@components/LoadingSpinner';
import UserHeader from '@components/layout/UserHeader';
import departmentsService, { Department } from '@services/departments/departmentsService';
import toast from 'react-hot-toast';

const DepartmentsPage: React.FC = () => {

    const navigate = useNavigate();
    const [departments, setDepartments] = useState<Department[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDeleting, setIsDeleting] = useState<string | null>(null);

    useEffect(() => {
        loadDepartments();
    }, []);

    const loadDepartments = async () => {
        try {
            setIsLoading(true);
            const data = await departmentsService.getDepartments();
            setDepartments(data);
        } catch (error) {
            toast.error('Error loading departments');
        } finally {
            setIsLoading(false);
        }
    };

    const handleAddDepartment = () => {
        navigate('/departments/create');
    };

    const handleEditDepartment = (id: string) => {
        navigate(`/departments/edit/${id}`);
    };

    const handleDeleteDepartment = async (id: string) => {
        if (!window.confirm('Are you sure you want to remove this department?')) {
            return;
        }

        try {
            setIsDeleting(id);
            await departmentsService.deleteDepartment(id);
            toast.success('Department removed successfully!');
            await loadDepartments(); // Recarregar a lista
        } catch (error) {
            toast.error('Error removing department');
        } finally {
            setIsDeleting(null);
        }
    };



    if (isLoading) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <LoadingSpinner size="lg" />
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50">
            <UserHeader
                title="Departments"
                subtitle="Company department management"
            >
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => navigate('/dashboard')}
                >
                    🏠 Dashboard
                </Button>
            </UserHeader>

            {/* Main Content */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="bg-white rounded-lg shadow">
                    {/* Header da tabela */}
                    <div className="px-6 py-4 border-b border-gray-200">
                        <div className="flex justify-between items-center">
                            <div className="flex items-center space-x-4">
                                <h2 className="text-lg font-medium text-gray-900">
                                    Department List
                                </h2>
                                <div className="flex space-x-2">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => navigate('/employees')}
                                    >
                                        👥 Employees
                                    </Button>
                                </div>
                            </div>
                            <Button
                                variant="primary"
                                size="sm"
                                onClick={handleAddDepartment}
                            >
                                + Add Department
                            </Button>
                        </div>
                    </div>

                    {/* Tabela */}
                    {departments.length > 0 ? (
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-blue-800">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">
                                            Name
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">
                                            Description
                                        </th>
                                        <th className="px-6 py-3 text-right text-xs font-medium text-white uppercase tracking-wider">
                                            Actions
                                        </th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {departments.map((department, index) => (
                                        <tr
                                            key={department.id}
                                            className={index % 2 === 0 ? 'bg-white' : 'bg-blue-50'}
                                        >
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="text-sm font-medium text-gray-900">
                                                    {department.name}
                                                </div>
                                            </td>
                                            <td className="px-6 py-4">
                                                <div className="text-sm text-gray-900">
                                                    {department.description}
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                                <div className="flex justify-end space-x-2">
                                                    <Button
                                                        variant="outline"
                                                        size="sm"
                                                        onClick={() => handleEditDepartment(department.id)}
                                                    >
                                                        Edit
                                                    </Button>
                                                    <Button
                                                        variant="outline"
                                                        size="sm"
                                                        onClick={() => handleDeleteDepartment(department.id)}
                                                        disabled={isDeleting === department.id}
                                                    >
                                                        {isDeleting === department.id ? (
                                                            <LoadingSpinner size="sm" />
                                                        ) : (
                                                            'Remove'
                                                        )}
                                                    </Button>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    ) : (
                        <div className="text-center py-12">
                            <div className="text-gray-400 mb-4">
                                <svg className="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                                </svg>
                            </div>
                            <h3 className="text-lg font-medium text-gray-900 mb-2">
                                No departments found
                            </h3>
                            <p className="text-gray-500">
                                Start by adding the first department to the company.
                            </p>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default DepartmentsPage;


