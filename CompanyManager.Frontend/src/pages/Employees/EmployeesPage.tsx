import React from 'react';
import { useNavigate } from 'react-router-dom';

import Button from '@components/ui/Button';
import UserHeader from '@components/layout/UserHeader';

const EmployeesPage: React.FC = () => {
    const navigate = useNavigate();




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
                        <Button variant="primary" size="sm">
                            + New Employee
                        </Button>
                    </div>

                    {/* Placeholder para lista de funcionários */}
                    <div className="text-center py-12">
                        <div className="text-gray-400 mb-4">
                            <svg className="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                            </svg>
                        </div>
                        <h3 className="text-lg font-medium text-gray-900 mb-2">
                            No employees found
                        </h3>
                        <p className="text-gray-500">
                            Start by adding the first employee to the company.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default EmployeesPage;


