import { useState } from 'react';

interface InputProps {
    label?: string;
    type?: 'text' | 'email' | 'password' | 'number' | 'date';
    value: string;
    onChange: (value: string) => void;
    placeholder?: string;
    required?: boolean;
    disabled?: boolean;
    error?: string;
    className?: string;
}

const Input: React.FC<InputProps> = ({
    label,
    type = 'text',
    value,
    onChange,
    placeholder,
    required = false,
    disabled = false,
    error,
    className = '',
}) => {
    return (
        <div className={`w-full ${className}`}>
            {label && (
                <label className="block text-sm font-medium text-black mb-1" htmlFor={`input-${label?.toLowerCase() || 'input'}`}>
                    {label}
                    {required && <span className="text-red-500 ml-1">*</span>}
                </label>
            )}
            <input
                id={`input-${label?.toLowerCase() || 'input'}`}
                type={type}
                value={value}
                onChange={(e) => onChange(e.target.value)}
                placeholder={placeholder}
                required={required}
                disabled={disabled}
                className={`w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-black placeholder-gray-500 ${error ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : ''}`}
            />
            {error && <span className="mt-1 text-sm text-red-600 block">{error}</span>}
        </div>
    );
};

export default Input;
