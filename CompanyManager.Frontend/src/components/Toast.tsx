import React from 'react';

interface ToastProps {
    message: string;
    type: 'success' | 'error' | 'warning' | 'info';
    onClose: () => void;
    duration?: number;
}

const Toast: React.FC<ToastProps> = ({
    message,
    type,
    onClose,
    duration = 5000,
}) => {
    React.useEffect(() => {
        const timer = setTimeout(() => {
            onClose();
        }, duration);

        return () => clearTimeout(timer);
    }, [duration, onClose]);

    const getToastClasses = () => {
        const baseClasses = 'p-4 rounded-md shadow-lg max-w-sm mx-auto';

        switch (type) {
            case 'success':
                return `${baseClasses} bg-green-100 text-green-800 border border-green-200`;
            case 'error':
                return `${baseClasses} bg-red-100 text-red-800 border border-red-200`;
            case 'warning':
                return `${baseClasses} bg-yellow-100 text-yellow-800 border border-yellow-200`;
            case 'info':
                return `${baseClasses} bg-blue-100 text-blue-800 border border-blue-200`;
            default:
                return `${baseClasses} bg-gray-100 text-gray-800 border border-gray-200`;
        }
    };

    return (
        <div className={getToastClasses()}>
            <div className="flex items-center justify-between">
                <span className="text-sm font-medium">{message}</span>
                <button
                    onClick={onClose}
                    className="ml-4 text-lg font-bold hover:opacity-70 focus:outline-none"
                >
                    ×
                </button>
            </div>
        </div>
    );
};

export default Toast;
