import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';
import UserHeader from '@components/layout/UserHeader';

const DashboardPage: React.FC = () => {
    const navigate = useNavigate();
    const { user } = useAuth();

    return (
        <div className="min-h-screen bg-gray-50">
            <UserHeader
                title="Dashboard"
                subtitle="Welcome to Company Manager"
            />

            <div className="flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
                <div className="max-w-md w-full space-y-8">
                    <div className="text-center">
                        <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
                            Welcome to Company Manager
                        </h2>
                        <p className="mt-2 text-sm text-gray-600">
                            {user?.firstName ? `Hello, ${user.firstName}!` : 'Hello!'}
                        </p>
                    </div>

                    <div className="bg-white shadow-lg rounded-lg p-6 space-y-6">
                        {/* Option 1: Create Department */}
                        <div className="border-2 border-blue-200 rounded-lg p-4 bg-blue-50">
                            <h3 className="text-lg font-semibold text-blue-900 mb-2">
                                1. If you don't have a department for this user, create the department
                            </h3>
                            <p className="text-blue-700 text-sm mb-4">
                                Start by creating a department to organize your company structure
                            </p>
                            <button
                                onClick={() => navigate('/departments/create')}
                                className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition-colors duration-200"
                            >
                                Create Department
                            </button>
                        </div>

                        {/* Option 2: Create Employee */}
                        <div className="border-2 border-green-200 rounded-lg p-4 bg-green-50">
                            <h3 className="text-lg font-semibold text-green-900 mb-2">
                                2. If you want to create a new employee and have a department, click here
                            </h3>
                            <p className="text-green-700 text-sm mb-4">
                                Add new employees to your existing departments
                            </p>
                            <button
                                onClick={() => navigate('/employees/create')}
                                className="w-full bg-green-600 hover:bg-green-700 text-white font-medium py-2 px-4 rounded-md transition-colors duration-200"
                            >
                                Create Employee
                            </button>
                        </div>

                        {/* Quick Actions */}
                        <div className="border-2 border-gray-200 rounded-lg p-4 bg-gray-50">
                            <h3 className="text-lg font-semibold text-gray-900 mb-3">
                                Quick Actions
                            </h3>
                            <div className="grid grid-cols-2 gap-3">
                                <button
                                    onClick={() => navigate('/departments')}
                                    className="bg-gray-600 hover:bg-gray-700 text-white font-medium py-2 px-3 rounded-md transition-colors duration-200 text-sm"
                                >
                                    View Departments
                                </button>
                                <button
                                    onClick={() => navigate('/employees')}
                                    className="bg-gray-600 hover:bg-gray-700 text-white font-medium py-2 px-3 rounded-md transition-colors duration-200 text-sm"
                                >
                                    View Employees
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default DashboardPage;

