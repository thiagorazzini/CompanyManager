import React from 'react';

interface ToastProps {
    message: string;
    type?: 'success' | 'error' | 'warning' | 'info';
    onClose?: () => void;
}

const Toast: React.FC<ToastProps> = ({
    message,
    type = 'info',
    onClose,
}) => {
    return (
        <div className={`toast toast-${type}`}>
            <span className="toast-message">{message}</span>
            {onClose && (
                <button className="toast-close" onClick={onClose}>
                    ×
                </button>
            )}
        </div>
    );
};

export default Toast;
