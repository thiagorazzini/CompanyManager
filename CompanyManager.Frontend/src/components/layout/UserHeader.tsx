import React from 'react';
import { useAuth } from '@hooks/useAuth';
import Button from '@components/ui/Button';

interface UserHeaderProps {
    title: string;
    subtitle?: string;
    children?: React.ReactNode;
}

const UserHeader: React.FC<UserHeaderProps> = ({ title, subtitle, children }) => {
    const { userProfile, logout } = useAuth();

    const handleLogout = () => {
        logout();
    };

    return (
        <div className="bg-white shadow-sm border-b border-gray-200">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between items-center py-4">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
                        {subtitle && (
                            <p className="text-sm text-gray-600">{subtitle}</p>
                        )}
                    </div>
                    <div className="flex items-center space-x-4">
                        {children}
                        <div className="text-right">
                            <p className="text-sm font-medium text-gray-900">
                                {userProfile?.firstName && userProfile?.lastName
                                    ? `${userProfile.firstName} ${userProfile.lastName}`
                                    : 'User'}
                            </p>
                            <p className="text-xs text-gray-500">
                                {userProfile?.email || 'Email not available'}
                            </p>
                        </div>
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={handleLogout}
                        >
                            Logout
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default UserHeader;
